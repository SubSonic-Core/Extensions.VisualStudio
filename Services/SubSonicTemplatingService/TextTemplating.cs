using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
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
using System.Windows.Threading;
using Thread = System.Threading.Thread;
using threadingTasks = System.Threading.Tasks;
using SubSonic.Core.VisualStudio.Host;
using Mono.TextTemplating;
using SubSonic.Core.TextWriters;
using SubSonic.Core.Utilities;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Services.Common;
using System.CodeDom.Compiler;
using Mono.TextTemplating.CodeCompilation;
using SubSonic.Core.VisualStudio.Common;

namespace SubSonic.Core.VisualStudio.Services
{
    public partial class SubSonicTemplatingService
        : IProcessTextTemplating
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

        EventHandler<ProcessTemplateEventArgs> TransformProcessCompletedHandler = null;

        public event EventHandler<ProcessTemplateEventArgs> TransformProcessCompleted
        {
            add
            {
                TransformProcessCompletedHandler = new EventHandler<ProcessTemplateEventArgs>(value);
            }
            remove
            {
                _ = (EventHandler<ProcessTemplateEventArgs>)Delegate.Remove(TransformProcessCompletedHandler, value);
                TransformProcessCompletedHandler = null;
            }
        }

        public bool LastInvocationRaisedErrors { get; set; }
        public bool MustUnloadAfterProcessingTemplate { get; private set; }
        

        public async threadingTasks.Task<string> ProcessTemplateAsync(string inputFilename, string content, ITextTemplatingCallback callback, object hierarchy, bool debugging = false)
        {
            string result = null;

            if (this is ITextTemplatingComponents Component)
            {
                Component.Hierarchy = hierarchy;
                Component.Callback = callback;
                Component.TemplateFile = inputFilename;
                LastInvocationRaisedErrors = false;
                Component.Host.SetFileExtension(SearchForLanguage(content, "C#") ? ".cs" : ".vb");

                TextWriter outputWindow = subSonicOutput.GetOutputTextWriter();

                using (CompiledTemplate ct = Engine.CompileTemplate(content, this))
                using (StringWriter output = new StringWriter(new StringBuilder()))
                using (StringWriter error = new StringWriter(new StringBuilder()))
                using (SplitOutputWriter splitOutput = new SplitOutputWriter(output, outputWindow))
                using (SplitOutputWriter splitError = new SplitOutputWriter(error, outputWindow))
                {
                    string path = Path.Combine(
                        Path.GetDirectoryName(typeof(SubSonicCoreVisualStudioAsyncPackage).Assembly.Location),
                        "T4HostProcess",
                        SubSonicMenuCommands.SubSonicHostProcessFIleName);

                    if (!File.Exists(path))
                    {
                        throw new FileNotFoundException(SubSonicCoreErrors.FileNotFound, SubSonicMenuCommands.SubSonicHostProcessFIleName);
                    }

                    Guid guid = Guid.NewGuid();

                    ProcessStartInfo psi = new ProcessStartInfo(path, guid.ToString())
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

                    RuntimeInfo runtime = RuntimeInfo.GetRuntime(package.HostOptions.RuntimeKind);

                    if (runtime.Kind == RuntimeKind.NetCore)
                    {
                        psi.Arguments = $"\"{psi.FileName}\" {psi.Arguments}";
                        psi.FileName = Path.GetFullPath(Path.Combine(runtime.RuntimeDirectory, "..", "..", "..", "dotnet"));
                    }

                    var task = ProcessUtilities.StartProcess(psi, splitOutput, splitError, out System.Diagnostics.Process theT4Host, CancellationTokenSource.Token);

                    if (!CancellationTokenSource.IsCancellationRequested)
                    {   // we have not been cancelled so let's continue
                        if (!debugging)
                        {   // if we are not debugging we can wait for the process to finish
                            await task.ConfigureAwait(false);

                            var errors = new CompilerErrorCollection();

                            async threadingTasks.Task ConsumeOutput(string s)
                            {
                                using (var sw = new StringReader(s))
                                {
                                    string line;
                                    while ((line = await sw.ReadLineAsync().ConfigureAwait(true)) != null)
                                    {
                                        CompilerError _error = null;//MSBuildErrorParser.TryParseLine(line);
                                        if (_error != null)
                                        {
                                            errors.Add(_error);
                                        }
                                    }
                                }
                            }

                            await ConsumeOutput(error.ToString()).ConfigureAwait(false);

                            result = output.ToString();
                        }
                        else
                        {   // when debugging this function will not return anything but null.
                            // the ui thread will be waiting for a callback from process to complete generation of file.
                            transformProcess = theT4Host;

                            return result;
                        }


                    }
                }
            }

            return result;
        }

        
        public string ProcessTemplate(string inputFile, string content, ITextTemplatingCallback callback = null, object hierarchy = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string result = "";

            if (this is ITextTemplatingComponents SubSonicComponents)
            {
                SubSonicComponents.Hierarchy = hierarchy;
                SubSonicComponents.Callback = callback;
                SubSonicComponents.TemplateFile = inputFile;

                SubSonicComponents.Host.SetFileExtension(SearchForLanguage(content, "C#") ? ".cs" : ".vb");

                Session = CreateSession();

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
            string result = null;

            references = Array.Empty<string>();

            if (this is ITextTemplatingComponents SubSonicComponents)
            {
                SubSonicComponents.Callback = callback;
                SubSonicComponents.TemplateFile = inputFile;

                LastInvocationRaisedErrors = false;

                result = SubSonicComponents.Engine.PreprocessTemplate(content, SubSonicComponents.Host, className, classNamespace, out string language, out references);

                if (language.Equals("C#", StringComparison.OrdinalIgnoreCase))
                {
                    SubSonicComponents.Host.SetFileExtension(".cs");
                }
                else if (language.Equals("VB", StringComparison.OrdinalIgnoreCase))
                {
                    SubSonicComponents.Host.SetFileExtension(".vb");
                }
                else
                {
                    SubSonicComponents.Host.SetFileExtension(DetectExtensionDirective(content));
                }
            }

            return result;
        }

        private string DetectExtensionDirective(string inputFileContent)
        {
            string extension = null;

            Match m = Regex.Match(inputFileContent,
               @"<#@\s*output(?:\s+encoding=""[.a-z0-9- ]*"")?(?:\s+extension=""([.a-z0-9- ]*)"")?(?:\s+encoding=""[.a-z0-9- ]*"")?\s*#>",
               RegexOptions.IgnoreCase);

            if (m.Success && m.Groups[1].Success)
            {
                extension = m.Groups[1].Value;

                if (extension != "" && !extension.StartsWith("."))
                {
                    extension = "." + extension;
                }
            }

            return extension;
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

            return ((currentErrors?.Count ?? 0) > 0);
        }
        #endregion

        private void OnTransformProcessCompleted(ProcessTemplateEventArgs args)
        {
            TransformProcessCompletedHandler?.Invoke(this, args);
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

        private void LogError(string message, bool isWarning = false)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            LogError(isWarning, message, -1, -1, TemplateFile);
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

                    if (package.DTE.Solution != null && fileName.IsNotNullOrEmpty())
                    {
                        item = package.DTE.Solution.FindProjectItem(Path.GetFileName(fileName));
                    }

                    if ((item != null) && item.ContainingProject != null)
                    {
                        service.GetProjectOfUniqueName(item.ContainingProject.UniqueName, out hierarchy);
                    }

                    ErrorTask task = new ErrorTask()
                    {
                        Category = TaskCategory.BuildCompile,
                        Document = item?.FileNames[0] ?? fileName,
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

        private TransformationRunFactory GetTransformationRunFactory(ParsedTemplate pt)
        {
            if (pt == null)
            {
                throw new ArgumentNullException(nameof(pt));
            }

            Guid guid = Guid.NewGuid();

            if (!isChannelRegistered)
            {
                try
                {
                    SubSonicTransformationRunFactory.RegisterChannel(guid);
                    isChannelRegistered = true;
                }
                catch(Exception ex)
                {
                    pt.LogError(string.Format(CultureInfo.CurrentCulture, SubSonicCoreErrors.RegisterIpcChannelFailed, ex), new Location(TemplateFile));
                }
            }

            if ((transformProcess == null) || (!RunFactoryIsAlive() || transformProcess.HasExited))
            {
                if (transformProcess != null && !transformProcess.HasExited)
                {
                    transformProcess.Kill();
                }

                

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
                    pt.LogError(ex.ToString(), new Location(TemplateFile));

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

        private void StartTransformation(string filename, IProcessTransformationRun transformationRun, Dispatcher uiDispatcher)
        {
            bool success = false;
            string processOutput = SubSonicCoreErrors.DebugErrorOutput;
            try
            {
                processOutput = this.runFactory.RunTransformation(transformationRun);
                success = !transformationRun.Errors.HasErrors;
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
                    ProcessTemplateEventArgs result = new ProcessTemplateEventArgs();
                    result.TemplateOutput = processOutput;
                    result.Succeeded = success;

#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
                    uiDispatcher.BeginInvoke((Action) (() => {
#pragma warning disable VSTHRD010 // Avoid legacy thread switching APIs
                        FinishTransformation(filename, result);
#pragma warning restore VSTHRD010 // Avoid legacy thread switching APIs
                    }), DispatcherPriority.Normal);
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs

                }
            }
        }

        private void FinishTransformation(string filename, ProcessTemplateEventArgs result)
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
                    this.OnTransformProcessCompleted(result);
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
