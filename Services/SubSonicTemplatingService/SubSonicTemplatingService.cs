using EnvDTE;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Microsoft.VisualStudio.Threading;
using SubSonic.Core.VisualStudio.Templating;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VSLangProj;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;

namespace SubSonic.Core.VisualStudio.Services
{
    public sealed partial class SubSonicTemplatingService
        : MarshalByRefObject
        , SSubSonicTemplatingService
        , ISubSonicTemplatingService
        , ITextTemplatingComponents
        , ITextTemplatingEngineHost
        , IServiceProvider
        , IDisposable
    {
        private readonly SubSonicCoreVisualStudioAsyncPackage package;
        private readonly ErrorListProvider errorListProvider;
        private readonly List<string> standardAssemblyReferences;
        private readonly List<string> standardImports;
        private AppDomain transformDomain;
        private System.Diagnostics.Process transformProcess;
        private readonly Regex foundAssembly = new Regex(@"[A-Z|a-z]:\\", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

        public SubSonicTemplatingService(SubSonicCoreVisualStudioAsyncPackage package)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Trace.WriteLine($"Constructing a new instance of {nameof(SubSonicTemplatingService)}.");
            this.standardAssemblyReferences = new List<string>();
            this.standardImports = new List<string>();
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            this.errorListProvider = new ErrorListProvider(package);
            this.errorListProvider.MaintainInitialTaskOrder = true;

            package.DTE.Events.SolutionEvents.AfterClosing += SolutionEvents_AfterClosing;
        }

        private void SolutionEvents_AfterClosing()
        {
            errorListProvider?.Tasks?.Clear();
        }

        private IAsyncServiceProvider AsyncServiceProvider => package;

        public async Task<object> InitializeAsync(CancellationToken cancellationToken)
        {
            await TaskScheduler.Default;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            //IVsShell shell = await serviceProvider.GetServiceAsync<SVsShell, IVsShell>();

            this.templating = await AsyncServiceProvider.GetServiceAsync<STextTemplating, ITextTemplating>();

            if (GetService(typeof(EnvDTE.DTE)) is EnvDTE.DTE dte)
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

                    foreach(var key in connectionManager.Connections.Keys)
                    {
                        manager.Add(key, new SubSonicDataConnection(connectionManager.Connections[key].Connection));
                    }

                    return manager;
                }

                return null;
            }
        }
        #endregion

        private ITextTemplating templating;
        private bool disposedValue;

        

        private ITextTemplatingComponents Components => this.templating as ITextTemplatingComponents;

        private ITextTemplatingEngineHost EngineHost => this.templating as ITextTemplatingEngineHost;

        private IDebugTextTemplating DebugTemplating => this.templating as IDebugTextTemplating;

        #region ITextTemplatingEngineHost
        public IList<string> StandardAssemblyReferences
        {
            get
            {
                if (!standardAssemblyReferences.Any())
                {
                    standardAssemblyReferences.AddRange(EngineHost.StandardAssemblyReferences
                    .Union(new[]
                    {
                        ResolveAssemblyReference(typeof(IDataConnection).Assembly.GetName().Name)
                    }).Distinct());
                }
                return standardAssemblyReferences;
            }
        }

        public IList<string> StandardImports
        {
            get
            {
                if (!standardImports.Any())
                {
                    standardImports.AddRange(EngineHost.StandardImports
                    .Union(new[]
                    {
                        typeof(IDataConnection).Namespace,
                        "Microsoft.VisualStudio.TextTemplating"
                    }).Distinct());
                }
                return standardImports;
            }
        }

        public string TemplateFile => EngineHost.TemplateFile;

        public object GetHostOption(string optionName)
        {
            return EngineHost.GetHostOption(optionName);
        }

        public bool LoadIncludeText(string requestFileName, out string content, out string location)
        {
            return EngineHost.LoadIncludeText(requestFileName, out content, out location);
        }

        public void LogErrors(CompilerErrorCollection errors)
        {
            EngineHost.LogErrors(errors);
        }

        public AppDomain ProvideTemplatingAppDomain(string content)
        {
            return transformDomain ?? (transformDomain = EngineHost.ProvideTemplatingAppDomain(content));
        }

        public string ResolveAssemblyReference(string assemblyReference)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string path = EngineHost.ResolveAssemblyReference(assemblyReference);

            if (path.Equals(assemblyReference, StringComparison.Ordinal) &&
                !foundAssembly.IsMatch(path))
            {   // failed to find the assembly, could it be referenced via a project reference?
                if (GetService(typeof(DTE)) is DTE dTE)
                {
                    foreach (Project project in dTE.Solution.Projects)
                    {
                        if (project.Object is VSProject vsProject)
                        {
                            path = ResolveAssemblyReferenceByProject(assemblyReference, vsProject.References);
                        }
                        else if (project.Object is VsWebSite.VSWebSite vsWebSite)
                        {
                            path = ResolveAssemblyReferenceByProject(assemblyReference, vsWebSite.References);
                        }
                    }
                }

                if (!foundAssembly.IsMatch(path))
                {
                    LogError(false, SubSonicCoreErrors.FileNotFound, -1, -1, $"{assemblyReference}.dll");
                }
            }

            return path;
        }

        private string ResolveAssemblyReferenceByProject(string assemblyReference, References references)
        {
            foreach (Reference reference in references)
            {
                if (reference.Name.Equals(assemblyReference, StringComparison.OrdinalIgnoreCase))
                {   // found the reference
                    return reference.Path;
                }
            }

            return assemblyReference;
        }

        private string ResolveAssemblyReferenceByProject(string assemblyReference, VsWebSite.AssemblyReferences references)
        {
            foreach (VsWebSite.AssemblyReference reference in references)
            {
                if (reference.Name.Equals(assemblyReference, StringComparison.OrdinalIgnoreCase))
                {   // found the reference
                    return reference.FullPath;
                }
            }

            return assemblyReference;
        }

        public Type ResolveDirectiveProcessor(string processorName)
        {
            return EngineHost.ResolveDirectiveProcessor(processorName);
        }

        public string ResolveParameterValue(string directiveId, string processorName, string parameterName)
        {
            return EngineHost.ResolveParameterValue(directiveId, processorName, parameterName);
        }

        public string ResolvePath(string path)
        {
            return EngineHost.ResolvePath(path);
        }

        public void SetFileExtension(string extension)
        {
            if (Callback != null)
            {
                Callback.SetFileExtension(extension);
            }

            extension = extension.Trim('.');

            ThreadHelper.ThrowIfNotOnUIThread();
            
            if (extension.Equals("cs", StringComparison.OrdinalIgnoreCase))
            {
                SqmFacade.T4SetFileExtensionCS();
            }
            else if (extension.Equals("vb", StringComparison.OrdinalIgnoreCase))
            {
                SqmFacade.T4SetFileExtensionVB();
            }
            else
            {
                SqmFacade.T4SetFileExtensionOther();
            }
        }

        public void SetOutputEncoding(Encoding encoding, bool fromOutputDirective)
        {
            Callback.SetOutputEncoding(encoding, fromOutputDirective);
        }

        internal void UnloadGenerationAppDomain()
        {
            if (transformDomain != null)
            {
                try
                {
                    if (transformDomain.GetLifetimeService() is ILease lease)
                    {
                        if (lease.CurrentState == LeaseState.Active)
                        {
                            AppDomain.Unload(transformDomain);
                        }
                    }
                }
                catch(AppDomainUnloadedException)
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
        public ITextTemplatingEngineHost Host => this;

        public ITextTemplatingEngine Engine => Components.Engine;

        public string InputFile { get => Components.InputFile; set => Components.InputFile = value; }
        public ITextTemplatingCallback Callback { get => Components.Callback; set => Components.Callback = value; }
        public object Hierarchy { get => Components.Hierarchy; set => Components.Hierarchy = value; }

        private IVsHierarchy VsHierarchy
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                return Components.Hierarchy as IVsHierarchy;
            }
        }
        #endregion

        public object GetService(Type serviceType)
        {
            object result = null;

            if (serviceType == typeof(ISubSonicTemplatingService))
            {
                return this;
            }
            else
            {
                if (templating is IServiceProvider service)
                {
                    result = service.GetService(serviceType);

                    if (result != null)
                    {
                        return result;
                    }
                }

                ThreadHelper.JoinableTaskFactory.Run(async delegate
                {
                    result = await AsyncServiceProvider.GetServiceAsync(serviceType);
                });
            }

            return result;
        }

        private void Dispose(bool disposing)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!disposedValue)
            {
                if (disposing)
                {
                    package.DTE.Events.SolutionEvents.AfterClosing -= SolutionEvents_AfterClosing;
                    errorListProvider.Dispose();
                    RemotingServices.Disconnect(this);
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
            ThreadHelper.ThrowIfNotOnUIThread();

            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
