using EnvDTE;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Mono.TextTemplating;
using Mono.VisualStudio.TextTemplating;
using SubSonic.Core.VisualStudio.Common;
using SubSonic.Core.VisualStudio.Templating;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Security;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VSLangProj;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;

namespace SubSonic.Core.VisualStudio.Services
{
    public sealed partial class SubSonicTemplatingService
        : MarshalByRefObject
        , SSubSonicTemplatingService
        , ITextTemplatingService
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
            this.Engine = new Mono.TextTemplating.TemplatingEngine();

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

        private bool disposedValue;

        #region ITextTemplatingEngineHost
        public IList<string> StandardAssemblyReferences
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (!standardAssemblyReferences.Any())
                {
                    standardAssemblyReferences.AddRange(new[]
                    {
                        ResolveAssemblyReference("System"),
                        ResolveAssemblyReference("System.Core"),
                        typeof(DependencyObject).Assembly.Location,
                        typeof(IDataConnection).Assembly.Location,
                        base.GetType().Assembly.Location
                    }.Distinct());
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
                    standardImports.AddRange(new[]
                    {
                        "System",
                        typeof(IDataConnection).Namespace,
                        "Mono.VisualStudio.TextTemplating"
                    }.Distinct());
                }
                return standardImports;
            }
        }

        public string TemplateFile { get; set; }

        public object GetHostOption(string optionName)
        {
            if (optionName.Equals(nameof(TemplateSettings.CachedTemplates), StringComparison.OrdinalIgnoreCase))
            {
                return package.HostOptions.CachedTemplates;
            }
            else if (optionName.Equals(nameof(TemplateSettings.CompilerOptions), StringComparison.OrdinalIgnoreCase))
            {
                return package.HostOptions.CompilerOptions;
            }
            else if (optionName.Equals(nameof(TemplateSettings.NoLinePragmas), StringComparison.OrdinalIgnoreCase))
            {
                return package.HostOptions.NoLinePragmas;
            }
            else if (optionName.Equals(nameof(TemplateSettings.RelativeLinePragmas), StringComparison.OrdinalIgnoreCase))
            {
                return package.HostOptions.RelativeLinePragmas;
            }

            return null;
        }

        public bool LoadIncludeText(string requestFileName, out string content, out string location)
        {
            throw new NotImplementedException();
        }

        public void LogErrors(CompilerErrorCollection errors)
        {
            foreach(CompilerError error in errors)
            {
#pragma warning disable VSTHRD010
                LogError(error.IsWarning, error.ErrorText, error.Line, error.Column, error.FileName);
#pragma warning restore VSTHRD010
            }
        }

        // app domain is not trully supported going forward
        public AppDomain ProvideTemplatingAppDomain(string content)
        {
            return AppDomain.CurrentDomain;
        }

        public string ResolveAssemblyReference(string assemblyReference)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string path = assemblyReference;

            if (!string.IsNullOrWhiteSpace(assemblyReference))
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    assemblyReference = await ExpandAllVariablesAsync(assemblyReference);
                });

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
                        LogError(string.Format(CultureInfo.CurrentCulture, SubSonicCoreErrors.AssemblyReferenceFailed, fileNotFound));
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
                        string vSinstallDir = GetVSInstallDir(SubSonicCoreVisualStudioAsyncPackage.Singleton.ApplicationRegistryRoot);
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
                if (!foundAssembly.IsMatch(path))
                {
                    // not sure if this willl be a problem, I worry about reference only assemblies.
                    // maybe ensure that the reference is from the nuget package and is a real boy.
                    // second check the project references
                    if (GetService(typeof(DTE)) is DTE dTE)
                    {
                        foreach (Project project in dTE.Solution.Projects)
                        {
                            if (project.Object is VSProject vsProject)
                            {
                                path = ResolveAssemblyReferenceByProject(assemblyReference, vsProject.References, GlobalAssemblyCacheHelper.strongNameRegEx.Match(assemblyReference));
                            }
                            else if (project.Object is VsWebSite.VSWebSite vsWebSite)
                            {
                                path = ResolveAssemblyReferenceByProject(assemblyReference, vsWebSite.References, GlobalAssemblyCacheHelper.strongNameRegEx.Match(assemblyReference));
                            }
                        }
                    }
                }
            }

            return path;
        }

        private async Task<string> ExpandAllVariablesAsync(string input)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (!string.IsNullOrWhiteSpace(input))
            {
                input = Environment.ExpandEnvironmentVariables(input);
                input = await Utilities.PathHelper.ExpandVsMacroVariablesAsync(input, VsHierarchy);
                input = await Utilities.PathHelper.ExpandProjectPropertiesAsync(input, VsHierarchy, this);
            }

            return input;
        }

        private string ResolveAssemblyReferenceByProject(string assemblyReference, References references, Match assembly)
        {
            foreach (Reference reference in references)
            {
                if (!assembly.Success && reference.Name.Equals(assemblyReference, StringComparison.OrdinalIgnoreCase))
                {   // found the reference
                    return reference.Path;
                }
                else if (reference.Name.Equals(assembly.Groups["name"].Value, StringComparison.OrdinalIgnoreCase) &&
                         Version.Parse(reference.Version) >= Version.Parse(assembly.Groups["version"].Value))
                {
                    return reference.Path;
                }
            }

            return assemblyReference;
        }

        private string ResolveAssemblyReferenceByProject(string assemblyReference, VsWebSite.AssemblyReferences references, Match assembly)
        {
            foreach (VsWebSite.AssemblyReference reference in references)
            {
                if (!assembly.Success && reference.Name.Equals(assemblyReference, StringComparison.OrdinalIgnoreCase))
                {   // found the reference
                    return reference.FullPath;
                }
                else if (reference.StrongName.Equals(assembly.Groups["name"].Value, StringComparison.OrdinalIgnoreCase))
                {
                    return reference.FullPath;
                }
            }

            return assemblyReference;
        }

        public Type ResolveDirectiveProcessor(string processorName)
        {
            throw new NotImplementedException();
        }

        public string ResolveParameterValue(string directiveId, string processorName, string parameterName)
        {
            throw new NotImplementedException();
        }

        public string ResolvePath(string path)
        {
            throw new NotImplementedException();
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

        public IDebugTextTemplatingEngine Engine { get; }
        ITextTemplatingEngine ITextTemplatingComponents.Engine => Engine;
        public string InputFile { get; set; }
        public ITextTemplatingCallback Callback { get; set; }
        public object Hierarchy { get; set; }

        private IVsHierarchy VsHierarchy
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                return Hierarchy as IVsHierarchy;
            }
        }
        #endregion

        #region ITextTemplatingSessionHost
        
        #endregion
        public object GetService(Type serviceType)
        {
            object result = null;

            if (serviceType.IsAssignableFrom(GetType()))
            {
                return this;
            }
            else
            {
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
