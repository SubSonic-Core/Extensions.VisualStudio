using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Microsoft.Win32;
using SubSonic.Core.VisualStudio.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Thread = System.Threading.Thread;

namespace SubSonic.Core.VisualStudio.Services
{
    public partial class SubSonicTemplatingService
        : IDebugTextTemplating
        , ITextTemplating
    {
        private int errorSessionDepth = 0;
        private IDictionary<string, int> currentErrors;
        private TextTemplatingSession transformationSession;
        private TransformationRunFactory runFactory;
        private bool transformationSessionImplicitlyCreated;
        private bool isChannelRegistered;
        private Thread debugThread;
        private static readonly Regex directiveParsingRegex = new Regex("template.*\\slanguage\\s*=\\s*\"(?<pvalue>.*?)(?<=[^\\\\](\\\\\\\\)*)\"", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        #region IDebugTextTemplating

        EventHandler<DebugTemplateEventArgs> DebugCompletedHandler = null;

        public event EventHandler<DebugTemplateEventArgs> DebugCompleted
        {
            add
            {
                DebugCompletedHandler = new EventHandler<DebugTemplateEventArgs>(value);
            }
            remove
            {
                _ = (EventHandler<DebugTemplateEventArgs>)Delegate.Remove(DebugCompletedHandler, value);
                DebugCompletedHandler = null;
            }
        }

        public bool LastInvocationRaisedErrors { get; set; }
        public bool MustUnloadAfterProcessingTemplate { get; private set; }
        

        public void DebugTemplateAsync(string inputFilename, string content, ITextTemplatingCallback callback, object hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (this is ITextTemplatingComponents SubSonicComponents)
            {
                SubSonicComponents.Hierarchy = hierarchy;
                SubSonicComponents.Callback = callback;
                SubSonicComponents.InputFile = inputFilename;
                LastInvocationRaisedErrors = false;
                SubSonicComponents.Host.SetFileExtension(SearchForLanguage(content, "C#") ? ".cs" : ".vb");

                try
                {
                    bool success = false;
                    IDebugTransformationRun debugTransformationRun = null;

                    try
                    {
                        runFactory = GetTransformationRunFactory();
                    }
                    catch(Exception)
                    {
                        try
                        {
                            runFactory = GetTransformationRunFactory();
                        }
                        catch(Exception ex)
                        {
                            object[] args = new[] { ex };
                            LogError(false, string.Format(CultureInfo.CurrentCulture, SubSonicCoreErrors.ExceptionStartingRunFactoryProcess, args), -1, -1, inputFilename);
                            runFactory = null;
                        }
                    }

                    if (runFactory == null)
                    {
                        this.LogError(false, SubSonicCoreErrors.ErrorStartingRunFactoryProcess, -1, -1, InputFile);
                        DebugTemplateEventArgs args = new DebugTemplateEventArgs();
                        args.TemplateOutput = SubSonicCoreErrors.DebugErrorOutput;
                        args.Succeeded = false;
                        this.OnDebugCompleted(args);
                        return;
                    }

                    if (SubSonicComponents.Engine is IDebugTextTemplatingEngine DebugEngine)
                    {
                        debugTransformationRun = DebugEngine.PrepareTransformationRun(content, SubSonicComponents.Host, runFactory);

                        if (debugTransformationRun != null)
                        {
                            try
                            {
                                foreach (EnvDTE.Process process in package.DTE.Debugger.LocalProcesses)
                                {
                                    if (process.ProcessID == this.transformProcess.Id)
                                    {
                                        process.Attach();
                                        success = true;
                                        break;
                                    }
                                }
                            }
                            catch (Exception exception2)
                            {
                                object[] args = new object[] { exception2 };
                                this.LogError(false, string.Format(CultureInfo.CurrentCulture, SubSonicCoreErrors.ExceptionAttachingToRunFactoryProcess, args), -1, -1, inputFilename);
                                success = false;
                            }
                        }
                        else
                        {
                            DebugTemplateEventArgs args = new DebugTemplateEventArgs();
                            args.TemplateOutput = SubSonicCoreErrors.DebugErrorOutput;
                            args.Succeeded = false;
                            this.OnDebugCompleted(args);
                            return;
                        }
                    }

                    if (success)
                    {
                        Dispatcher uiDispatcher = Dispatcher.CurrentDispatcher;
                        this.debugThread = new Thread(() => StartTransformation(inputFilename, debugTransformationRun, uiDispatcher));
                        this.debugThread.Start();
                    }
                    else
                    {
                        this.LogError(false, SubSonicCoreErrors.ErrorAttachingToRunFactoryProcess, -1, -1, inputFilename);
                        DebugTemplateEventArgs args = new DebugTemplateEventArgs();
                        args.TemplateOutput = SubSonicCoreErrors.DebugErrorOutput;
                        args.Succeeded = false;
                        this.OnDebugCompleted(args);
                    }
                    return;

                }
                catch (Exception exception3)
                {
                    this.LogError(false, exception3.ToString(), -1, -1, inputFilename);
                    DebugTemplateEventArgs args = new DebugTemplateEventArgs();
                    args.TemplateOutput = SubSonicCoreErrors.DebugErrorOutput;
                    args.Succeeded = false;
                    OnDebugCompleted(args);
                    return;
                }
            }
        }

        
        public string ProcessTemplate(string inputFile, string content, ITextTemplatingCallback callback = null, object hierarchy = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string result = "";

            if (this is ITextTemplatingComponents SubSonicComponents)
            {
                SubSonicComponents.Hierarchy = hierarchy;
                SubSonicComponents.Callback = callback;
                SubSonicComponents.InputFile = inputFile;

                SubSonicComponents.Host.SetFileExtension(SearchForLanguage(content, "C#") ? ".cs" : ".vb");

                result = SubSonicComponents.Engine.ProcessTemplate(content, SubSonicComponents.Host);

                if ((errorSessionDepth == 0) && MustUnloadAfterProcessingTemplate)
                {
                    MustUnloadAfterProcessingTemplate = false;
                    UnloadGenerationAppDomain();
                }

                // SqmFacade is a DTE wrapper that can send additional commands to VS
                if (SearchForLanguage(content, "C#"))
                {
                    SqmFacade.T4PreprocessTemplateCS();
                }
                else if (SearchForLanguage(content, "VB"))
                {
                    SqmFacade.T4PreprocessTemplateVB();
                }
            }

            return result;
        }

        public string PreprocessTemplate(string inputFile, string content, ITextTemplatingCallback callback, string className, string classNamespace, out string[] references)
        {
            return templating.PreprocessTemplate(inputFile, content, callback, className, classNamespace, out references);
        }

        public void BeginErrorSession()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (errorSessionDepth == 0)
            {
                if (currentErrors == null)
                {
                    currentErrors = new Dictionary<string, int>();
                }
                else
                {
                    currentErrors.Clear();
                }
                errorListProvider.Tasks.Clear();
                if ((transformationSession == null) && !transformationSessionImplicitlyCreated)
                {
                    transformationSession = new TextTemplatingSession();
                    transformationSessionImplicitlyCreated = true;
                }
            }
            errorSessionDepth++;

            DebugTemplating.BeginErrorSession();
        }

        public bool EndErrorSession()
        {
            errorSessionDepth--;

            if (MustUnloadAfterProcessingTemplate)
            {
                MustUnloadAfterProcessingTemplate = false;
                UnloadGenerationAppDomain();
            }
            if (errorSessionDepth == 0 && transformationSessionImplicitlyCreated)
            {
                transformationSession = null;
                transformationSessionImplicitlyCreated = false;
            }

            bool result = DebugTemplating.EndErrorSession();

            return ((currentErrors?.Count ?? 0) > 0) || result;
        }
        #endregion

        private void OnDebugCompleted(DebugTemplateEventArgs args)
        {
            DebugCompletedHandler?.Invoke(this, args);
        }

        private bool SearchForLanguage(string templateContent, string language)
        {
            bool flag = false;
            MatchCollection matchs = directiveParsingRegex.Matches(templateContent);
            if (matchs.Count > 0)
            {
                flag = string.Equals(matchs[0].Groups["pvalue"].Value, language, StringComparison.OrdinalIgnoreCase);
            }
            return flag;
        }

        private void LogError(bool isWarning, string message, int line, int column, string fileName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            int num = 0;
            line = line > 0 ? --line : line;
            column = column > 0 ? --column : column;

            if ((currentErrors == null) || !currentErrors.TryGetValue(message, out num))
            {
                if (currentErrors != null)
                {
                    currentErrors[message] = 1;
                }

                Callback?.ErrorCallback(isWarning, message, line, column);

                if (GetService(typeof(IVsSolution)) is IVsSolution service)
                {
                    IVsHierarchy hierarchy = null;
                    ProjectItem item = null;

                    if (package.DTE.Solution != null)
                    {
                        item = package.DTE.Solution.FindProjectItem(fileName);
                    }

                    if ((item != null) && item.ContainingProject != null)
                    {
                        service.GetProjectOfUniqueName(item.ContainingProject.UniqueName, out hierarchy);
                    }

                    ErrorTask task = new ErrorTask()
                    {
                        Category = TaskCategory.BuildCompile,
                        Document = fileName,
                        HierarchyItem = hierarchy,
                        CanDelete = false,
                        Column = column,
                        Line = line,
                        Text = message,
                        ErrorCategory = isWarning ? TaskErrorCategory.Warning : TaskErrorCategory.Error
                    };

                    task.Navigate += Task_Navigate;

                    errorListProvider.Tasks.Add(task);
                }
            }
        }

        private void Task_Navigate(object sender, EventArgs e)
        {
            ErrorTask task = sender as ErrorTask;
            if ((task != null) && (!string.IsNullOrEmpty(task.Document) && File.Exists(task.Document)))
            {
                IVsUIHierarchy hierarchy;
                uint num;
                IVsWindowFrame frame;
                VsShellUtilities.OpenDocument(package, task.Document, Guid.Empty, out hierarchy, out num, out frame);
                if (frame != null)
                {
                    task.HierarchyItem = hierarchy;
                    errorListProvider.Refresh();
                    IVsTextView textView = VsShellUtilities.GetTextView(frame);
                    if (textView != null)
                    {
                        textView.SetSelection(task.Line, task.Column, task.Line, task.Column);
                    }
                }
            }
        }

        private TransformationRunFactory GetTransformationRunFactory()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!isChannelRegistered)
            {
                try
                {
                    TransformationRunFactory.RegisterIpcChannel();
                    isChannelRegistered = true;
                }
                catch(Exception ex)
                {
                    LogError(false, string.Format(CultureInfo.CurrentCulture, SubSonicCoreErrors.RegisterIpcChannelFailed, ex), -1, -1, InputFile);
                }
            }

            if ((transformProcess == null) || (!RunFactoryIsAlive() || transformProcess.HasExited))
            {
                if (transformProcess != null && !transformProcess.HasExited)
                {
                    transformProcess.Kill();
                }

                Guid guid = Guid.NewGuid();

                ProcessStartInfo startInfo = new ProcessStartInfo(Path.Combine(GetVSInstallDir(package.ApplicationRegistryRoot), "T4VSHostProcess.exe"), guid.ToString())
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                try
                {
                    transformProcess = System.Diagnostics.Process.Start(startInfo);
                }
                catch(Exception ex)
                {
                    LogError(false, ex.ToString(), -1, -1, InputFile);

                    return default;
                }

                Stopwatch stopwatch = Stopwatch.StartNew();

                while(true)
                {
                    if ((this.runFactory != null) || (stopwatch.ElapsedMilliseconds >= 0x2710L))
                    {
                        stopwatch.Stop();
                        break;
                    }
                    Thread.Sleep(10);
                    try
                    {
                        runFactory = (TransformationRunFactory)Activator.GetObject(typeof(TransformationRunFactory), "ipc://TransformationRunFactoryService" + guid.ToString() + "/TransformationRunFactory");
                        if (!this.RunFactoryIsAlive())
                        {
                            this.runFactory = null;
                        }
                    }
                    catch (RemotingException)
                    {
                        this.runFactory = null;
                    }
                }
            }

            return runFactory;
        }

        private void StartTransformation(string filename, IDebugTransformationRun transformationRun, Dispatcher uiDispatcher)
        {
            bool success = false;
            string debugErrorOutput = SubSonicCoreErrors.DebugErrorOutput;
            bool debug_success = false;
            try
            {
                debugErrorOutput = this.runFactory.RunTransformation(transformationRun);
                debug_success = !transformationRun.Errors.HasErrors;
                this.LogErrors(transformationRun.Errors);
            }
            catch (RemotingException exception)
            {
                this.LogError(false, exception.ToString(), -1, -1, filename);
            }
            catch (ThreadAbortException)
            {
                success = true;
            }
            finally
            {
                if (!success)
                {
                    DebugTemplateEventArgs args1 = new DebugTemplateEventArgs();
                    args1.TemplateOutput = debugErrorOutput;
                    args1.Succeeded = debug_success;
                    DebugTemplateEventArgs result = args1;



#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
                    uiDispatcher.BeginInvoke((Action) (() => FinishTransformation(filename, result)), DispatcherPriority.Normal);
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs
                }
            }
        }

        private void FinishTransformation(string filename, DebugTemplateEventArgs result)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (package.DTE.Debugger.DebuggedProcesses != null)
            {
                try
                {
                    foreach (EnvDTE.Process process in package.DTE.Debugger.DebuggedProcesses)
                    {
                        if (process.ProcessID == this.transformProcess.Id)
                        {
                            process.Detach(true);
                            break;
                        }
                    }
                }
                catch (Exception exception)
                {
                    this.LogError(true, exception.ToString(), -1, -1, filename);
                }
                finally
                {
                    this.OnDebugCompleted(result);
                }
            }
        }

        private string GetVSInstallDir(RegistryKey applicationRoot)
        {
            string str = string.Empty;
            if (applicationRoot != null)
            {
                str = applicationRoot.GetValue("InstallDir") as string;
            }
            return ((str != null) ? str.Replace(@"\\", @"\") : string.Empty);
        }

        private bool RunFactoryIsAlive()
        {
            try
            {
                runFactory?.ToString();
            }
            catch(RemotingException)
            {
                runFactory = null;
            }

            return runFactory != null;
        }
    }
}
