using EnvDTE;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Microsoft.Win32;
using Mono.TextTemplating;
using Mono.TextTemplating.CodeCompilation;
using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using SubSonic.Core.Remoting;
using SubSonic.Core.VisualStudio.Common;
using SubSonic.Core.VisualStudio.Templating;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VSLangProj;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
using ThreadingTasks = System.Threading.Tasks;
using VsShell = Microsoft.VisualStudio.Shell;

namespace SubSonic.Core.VisualStudio.Services
{
    public sealed partial class SubSonicTemplatingService
        : MarshalByRefObject
        , SSubSonicTemplatingService
        , ITextTemplatingComponents
        , IProcessTextTemplating
        , IServiceProvider
        , IDisposable
    {
        private static SubSonicTemplatingService singleton;
        private readonly SubSonicCoreVisualStudioAsyncPackage package;
        private readonly VsShell.ErrorListProvider errorListProvider;
        internal readonly SubSonicOutputWriter subSonicOutput;
        private AppDomain transformDomain;
        private System.Diagnostics.Process transformProcess;
        private readonly Regex foundAssembly = new Regex(@"[A-Z|a-z]:\\", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

        public SubSonicTemplatingService(SubSonicCoreVisualStudioAsyncPackage package)
        {
            Trace.WriteLine($"Constructing a new instance of {nameof(SubSonicTemplatingService)}.");

            VsShell.ThreadHelper.ThrowIfNotOnUIThread();

            this.package = package ?? throw new ArgumentNullException(nameof(package));
            this.errorListProvider = new VsShell.ErrorListProvider(package)
            {
                MaintainInitialTaskOrder = true
            };
            this.subSonicOutput = new SubSonicOutputWriter(package);

            this.Engine = new TemplatingEngine();
            this.transformationHost = new RemoteTransformationHost();

            this.transformationHost.GetHostOptionEventHandler += TransformationHost_GetHostOptionEventHandler;
            this.transformationHost.ResolveAssemblyReferenceEventHandler += TransformationHost_ResolveAssemblyReferenceEventHandler;
            this.transformationHost.ExpandAllVariablesEventHandler += TransformationHost_ExpandAllVariablesEventHandler;
            this.transformationHost.ResolveParameterValueEventHandler += TransformationHost_ResolveParameterValueEventHandler;

            singleton = this;

            package.DTE.Events.SolutionEvents.AfterClosing += SolutionEvents_AfterClosing;
        }

        private string TransformationHost_ResolveParameterValueEventHandler(object sender, Host.ResolveParameterValueEventArgs args)
        {
            if (ConnectionManager.ContainsKey(args.ParameterKey.ParameterName))
            {
                return ConnectionManager[args.ParameterKey.ParameterName].SafeConnectionString;
            }

            return default;
        }

        private string TransformationHost_ExpandAllVariablesEventHandler(object sender, Host.ExpandAllVariablesEventArgs args)
        {
            return VsShell.ThreadHelper.JoinableTaskFactory.Run(async () =>
               await ExpandAllVariablesAsync(args.FilePath)
           );
        }

        private string TransformationHost_ResolveAssemblyReferenceEventHandler(object sender, Host.ResolveAssemblyReferenceEventArgs args)
        {
            return VsShell.ThreadHelper.JoinableTaskFactory.Run(async () =>
                await ResolveAssemblyReferenceAsync(args.AssemblyReference)
            );
        }

        private static SubSonicCoreVisualStudioAsyncPackage Package()
        {
            return SubSonicCoreVisualStudioAsyncPackage.Singleton;
        }

        private object TransformationHost_GetHostOptionEventHandler(object sender, Host.GetHostOptionEventArgs args)
        {
            if (Package() != null)
            {
                if (args.OptionName.Equals(nameof(TemplateSettings.CachedTemplates), StringComparison.OrdinalIgnoreCase))
                {
                    return Package().HostOptions.CachedTemplates;
                }
                else if (args.OptionName.Equals(nameof(TemplateSettings.CompilerOptions), StringComparison.OrdinalIgnoreCase))
                {
                    return Package().HostOptions.CompilerOptions;
                }
                else if (args.OptionName.Equals(nameof(TemplateSettings.NoLinePragmas), StringComparison.OrdinalIgnoreCase))
                {
                    return Package().HostOptions.LinePragmas;
                }
                else if (args.OptionName.Equals(nameof(TemplateSettings.UseRelativeLinePragmas), StringComparison.OrdinalIgnoreCase))
                {
                    return Package().HostOptions.UseRelativeLinePragmas;
                }
                else if (args.OptionName.Equals(nameof(TemplateSettings.Log), StringComparison.OrdinalIgnoreCase))
                {   // supply the engine with a textwriter that can output to the output pane.
                    return subSonicOutput.GetOutputTextWriter();
                }
                else if (args.OptionName.Equals(nameof(TemplateSettings.RuntimeKind), StringComparison.OrdinalIgnoreCase))
                {
                    return Package().HostOptions.RuntimeKind;
                }
            }

            return null;
        }

        public static SubSonicTemplatingService Singleton => singleton;

        private void SolutionEvents_AfterClosing()
        {
            Package()?.CancellationTokenSource.Cancel();
            errorListProvider?.Tasks?.Clear();
        }

        private IAsyncServiceProvider AsyncServiceProvider => package;

        public async Task<object> InitializeAsync(CancellationToken cancellationToken)
        {
            await TaskScheduler.Default;
            await VsShell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            await subSonicOutput.InitializeAsync("SubSonic Core", cancellationToken);

            await transformationHost.InitializeAsync();

            if (GetService(typeof(DTE)) is DTE dte)
            {
                SqmFacade.Initialize(dte);
            }

            return this;
        }

        #region ISubSonicTemplatingService implemenation
        public IConnectionManager ConnectionManager
        {
            get
            {
                if (GetService(typeof(IVsDataExplorerConnectionManager)) is IVsDataExplorerConnectionManager connectionManager)
                {
                    SubSonicConnectionManager manager = new SubSonicConnectionManager();

                    foreach (var key in connectionManager.Connections.Keys)
                    {
                        manager.Add(key.ToConnectionKey(), new SubSonicDataConnection(connectionManager.Connections[key].Connection));
                    }

                    return manager;
                }

                return null;
            }
        }
        #endregion

        private bool disposedValue;

        #region ITextTemplatingEngineHost

        public async Task<string> ResolveAssemblyReferenceAsync(string assemblyReference)
        {
            await VsShell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(Package().CancellationTokenSource.Token);

            string path = assemblyReference;

            if (!string.IsNullOrWhiteSpace(assemblyReference))
            {
                assemblyReference = await SubSonicTemplatingService.Singleton.ExpandAllVariablesAsync(path);

                if (Path.IsPathRooted(assemblyReference))
                {   // assembly reference is path rooted
                    Zone zone = Zone.CreateFromUrl(new Uri(assemblyReference).AbsoluteUri);
                    if (zone.SecurityZone == SecurityZone.Trusted || zone.SecurityZone == SecurityZone.MyComputer)
                    {
                        path = assemblyReference;
                    }
                    else
                    {
                        string fileNotFound = string.Format(CultureInfo.CurrentCulture, SubSonicCoreErrors.FileNotFound, assemblyReference);
                        _ = LogErrorAsync(string.Format(CultureInfo.CurrentCulture, SubSonicCoreErrors.AssemblyReferenceFailed, fileNotFound));
                    }
                }

                // we have not found a thing let's look at the GAC
                if (!foundAssembly.IsMatch(path))
                {
                    path = GlobalAssemblyCacheHelper.GetLocation(assemblyReference);
                }

                // still have not found anything let's check the public assemblies folder
                if (!foundAssembly.IsMatch(path))
                {
                    string extension = Path.GetExtension(assemblyReference);

                    bool executable = extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) || extension.Equals(".exe", StringComparison.OrdinalIgnoreCase);

                    if (SubSonicCoreVisualStudioAsyncPackage.Singleton != null)
                    {
                        string vSinstallDir = GetVSInstallDir(Package().ApplicationRegistryRoot);
                        if (!string.IsNullOrEmpty(vSinstallDir))
                        {
                            string code_base = Path.Combine(Path.Combine(vSinstallDir, "PublicAssemblies"), assemblyReference);

                            if (File.Exists(code_base))
                            {
                                path = code_base;
                            }
                            if (!executable)
                            {
                                code_base = $"{code_base}.dll";
                                if (File.Exists(code_base))
                                {
                                    path = code_base;
                                }
                            }
                        }
                    }
                }                

                // if not found look at the project references
                //if (!foundAssembly.IsMatch(path))
                //{
                //    // not sure if this willl be a problem, I worry about reference only assemblies.
                //    // maybe ensure that the reference is from the nuget package and is a real boy.
                //    // second check the project references
                //    if (Package()?.GetService<DTE>() is DTE dTE)
                //    {
                //        foreach (Project project in dTE.Solution.Projects)
                //        {
                //            if (project.Object is VSProject vsProject)
                //            {
                //                path = ResolveAssemblyReferenceByProject(assemblyReference, vsProject.References, GlobalAssemblyCacheHelper.strongNameRegEx.Match(assemblyReference));
                //            }
                //            else if (project.Object is VsWebSite.VSWebSite vsWebSite)
                //            {
                //                path = ResolveAssemblyReferenceByProject(assemblyReference, vsWebSite.References, GlobalAssemblyCacheHelper.strongNameRegEx.Match(assemblyReference));
                //            }
                //        }
                //    }
                //}
            }

            return path;
        }

        //private string ResolveAssemblyReferenceByProject(string assemblyReference, References references, Match assembly)
        //{
        //    foreach (Reference reference in references)
        //    {
        //        if (!assembly.Success && reference.Name.Equals(assemblyReference, StringComparison.OrdinalIgnoreCase))
        //        {   // found the reference
        //            if (VerifyAssemblyValidity(reference.Path, out string path))
        //            {
        //                return path;
        //            }
        //            continue;
        //        }
        //        else if (reference.Name.Equals(assembly.Groups["name"].Value, StringComparison.OrdinalIgnoreCase) &&
        //                 Version.Parse(reference.Version) >= Version.Parse(assembly.Groups["version"].Value))
        //        {
        //            if (VerifyAssemblyValidity(reference.Path, out string path))
        //            {
        //                return path;
        //            }
        //            continue;
        //        }
        //    }

        //    return assemblyReference;
        //}

        //private string ResolveAssemblyReferenceByProject(string assemblyReference, VsWebSite.AssemblyReferences references, Match assembly)
        //{
        //    foreach (VsWebSite.AssemblyReference reference in references)
        //    {
        //        if (!assembly.Success && reference.Name.Equals(assemblyReference, StringComparison.OrdinalIgnoreCase))
        //        {   // found the reference
        //            if (VerifyAssemblyValidity(reference.FullPath, out string path))
        //            {
        //                return path;
        //            }
        //            continue;
        //        }
        //        else if (reference.StrongName.Equals(assembly.Groups["name"].Value, StringComparison.OrdinalIgnoreCase))
        //        {
        //            if (VerifyAssemblyValidity(reference.FullPath, out string path))
        //            {
        //                return path;
        //            }
        //            continue;
        //        }
        //    }

        //    return assemblyReference;
        //}

        //private bool VerifyAssemblyValidity(string path, out string assemblyPath)
        //{
        //    assemblyPath = path;

        //    if (assemblyPath.Contains("\\ref\\"))
        //    {   // strong possibility that this is a reference assembly
        //        // banking on path structure being logical and in line with package build guidelines.
                
        //        int index = assemblyPath.IndexOf("\\ref\\");
        //        string[] _path = new[]
        //        {
        //            assemblyPath.Substring(0, index),
        //            assemblyPath.Substring(index + "\\ref\\".Length)
        //        };

        //        if (Directory.Exists(Path.Combine(_path[0],"runtimes")))
        //        {  
        //            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        //            {
        //                assemblyPath = Path.Combine(_path[0], "runtimes\\win\\lib", _path[1]);
        //            }
        //            else if (Environment.OSVersion.Platform == PlatformID.Unix)
        //            {
        //                assemblyPath = Path.Combine(_path[0], "runtimes\\unix\\lib", _path[1]);
        //            }
        //        }
        //        else
        //        {
        //            assemblyPath = assemblyPath.Replace("\\ref\\", "\\lib\\");
        //        }                

        //        return File.Exists(assemblyPath);
        //    }
        //    return true;
        //}

        private string GetVSInstallDir(RegistryKey applicationRoot)
        {
            string str = string.Empty;
            if (applicationRoot != null)
            {
                str = applicationRoot.GetValue("InstallDir") as string;
            }
            return ((str != null) ? str.Replace(@"\\", @"\") : string.Empty);
        }

        public void LogErrors(CompilerErrorCollection errors)
        {
            _ = LogErrorsAsync(errors);
        }

        public async ThreadingTasks.Task LogErrorsAsync(CompilerErrorCollection errors)
        {
            foreach (CompilerError error in errors)
            {
                await LogErrorAsync(error.ErrorText, new Location(error.FileName, error.Line, error.Column), error.IsWarning);
            }
        }
        

        public async Task<string> ExpandAllVariablesAsync(string input)
        {
            await VsShell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (!string.IsNullOrWhiteSpace(input))
            {
                input = Environment.ExpandEnvironmentVariables(input);
                input = await Utilities.PathHelper.ExpandVsMacroVariablesAsync(input, VsHierarchy);
                input = await Utilities.PathHelper.ExpandProjectPropertiesAsync(input, VsHierarchy, this);
            }

            return input;
        }

        

        internal void UnloadGenerationAppDomain()
        {
            if (transformDomain != null)
            {
                try
                {
                    AppDomain.Unload(transformDomain);
                }
                catch (AppDomainUnloadedException)
                {
                }
                finally
                {
                    transformDomain = null;
                }
            }
            if (transformProcess != null)
            {
                try
                {
                    transformProcess.Kill();
                    transformProcess = null;
                    if ((debugThread != null) && this.debugThread.IsAlive)
                    {
                        debugThread.Abort();
                    }
                }
                catch (Win32Exception)
                {
                }
                catch (InvalidOperationException)
                {
                }
                catch (ThreadAbortException)
                {
                }
                catch (ThreadStateException)
                {
                }
            }
        }
        #endregion

        #region ITextTemplatingComponents
        private readonly RemoteTransformationHost transformationHost;

        public ITextTemplatingEngineHost Host => transformationHost;

        public IProcessTextTemplatingEngine Engine { get; }

        private TemplatingEngine MonoEngine => Engine as TemplatingEngine;

        public string TemplateFile { get => transformationHost.TemplateFile; set => transformationHost.TemplateFile = value; }

        ITextTemplatingEngine ITextTemplatingComponents.Engine => Engine;
        public ITextTemplatingCallback Callback { get; set; }
        public object Hierarchy { get; set; }

        private IVsHierarchy VsHierarchy
        {
            get
            {
                VsShell.ThreadHelper.ThrowIfNotOnUIThread();

                return Hierarchy as IVsHierarchy;
            }
        }
        #endregion

        public object GetService(Type serviceType)
        {
            object result;

            if (serviceType.IsAssignableFrom(GetType()))
            {
                result = this;
            }
            else if (serviceType is ITextTemplatingEngineHost ||
                     serviceType is ITextTemplatingSessionHost)
            {
                return transformationHost;
            }
            else if (serviceType is ITextTemplatingSession)
            {
                return transformationHost.Session;
            }
            else
            {
#pragma warning disable VSTHRD104 // Offer async methods
                result = VsShell.ThreadHelper.JoinableTaskFactory.Run(async () => await AsyncServiceProvider.GetServiceAsync(serviceType));
#pragma warning restore VSTHRD104 // Offer async methods
            }

            return result;
        }

        private void Dispose(bool disposing)
        {
            VsShell.ThreadHelper.ThrowIfNotOnUIThread();

            if (!disposedValue)
            {
                if (disposing)
                {
                    transformationHost.ResolveAssemblyReferenceEventHandler -= TransformationHost_ResolveAssemblyReferenceEventHandler;
                    transformationHost.GetHostOptionEventHandler -= TransformationHost_GetHostOptionEventHandler;
                    transformationHost.ExpandAllVariablesEventHandler -= TransformationHost_ExpandAllVariablesEventHandler;
                    transformationHost.ResolveParameterValueEventHandler -= TransformationHost_ResolveParameterValueEventHandler;
                    package.DTE.Events.SolutionEvents.AfterClosing -= SolutionEvents_AfterClosing;
                    errorListProvider.Dispose();
                    subSonicOutput.Dispose();
                    RemotingServices.Disconnect(new Uri($"ipc://{TransformationRunFactory.TransformationRunFactoryService}"));
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SubSonicTemplatingService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            VsShell.ThreadHelper.ThrowIfNotOnUIThread();

            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
