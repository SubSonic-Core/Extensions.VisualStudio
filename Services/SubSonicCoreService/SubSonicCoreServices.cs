using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.IO;
using Microsoft.VisualStudio.Threading;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.TextTemplating;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.CodeDom.Compiler;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using SubSonic.Core.VisualStudio.Templating;
using Microsoft.VisualStudio.Data.Services;

namespace SubSonic.Core.VisualStudio.Services
{
    public partial class SubSonicCoreService
        : MarshalByRefObject
        , SSubSonicCoreService
        , ISubSonicCoreService
        , ITextTemplatingComponents
        , ITextTemplatingEngineHost
        , IServiceProvider
        , IDisposable
    {
        private readonly IAsyncServiceProvider serviceProvider;
        
        public SubSonicCoreService(IAsyncServiceProvider serviceProvider)
        {
            Trace.WriteLine($"Constructing a new instance of {nameof(SubSonicCoreService)}.");

            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<object> InitializeAsync(CancellationToken cancellationToken)
        {
            await TaskScheduler.Default;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            //IVsShell shell = await serviceProvider.GetServiceAsync<SVsShell, IVsShell>();

            this.templating = await serviceProvider.GetServiceAsync<STextTemplating, ITextTemplating>();

            if (GetService(typeof(EnvDTE.DTE)) is EnvDTE.DTE dte)
            {
                SqmFacade.Initialize(dte);
            }

            return this;
        }

        #region ISubSonicCoreService implemenation
        public IConnectionManager ConnectionManager
        {
            get
            {
                if (GetService(typeof(IVsDataExplorerConnectionManager)) is IVsDataExplorerConnectionManager connectionManager)
                {
                    return new VsDataExplorerConnectionWrapper(connectionManager);
                }

                return null;
            }
        }
        #endregion

        private ITextTemplating templating;

        public event EventHandler<DebugTemplateEventArgs> DebugCompleted
        {
            add
            {
                DebugTemplating.DebugCompleted += value;
            }
            remove
            {
                DebugTemplating.DebugCompleted -= value;
            }
        }

        private ITextTemplatingComponents Components => this.templating as ITextTemplatingComponents;

        private ITextTemplatingEngineHost EngineHost => this.templating as ITextTemplatingEngineHost;

        private IDebugTextTemplating DebugTemplating => this.templating as IDebugTextTemplating;

        #region ITextTemplatingEngineHost
        public IList<string> StandardAssemblyReferences
        {
            get
            {
                return EngineHost.StandardAssemblyReferences
                    .Union(new[]
                    {
                        ResolveAssemblyReference(typeof(IDataConnection).Assembly.GetName().Name)
                    })
                    .ToList();
            }
        }

        public IList<string> StandardImports
        {
            get
            {
                return EngineHost.StandardImports
                    .Union(new[]
                    {
                        typeof(IDataConnection).Namespace,
                        "Microsoft.VisualStudio.TextTemplating"
                    })
                    .ToList();
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
            return EngineHost.ProvideTemplatingAppDomain(content);
        }

        public string ResolveAssemblyReference(string assemblyReference)
        {
            return EngineHost.ResolveAssemblyReference(assemblyReference);
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
            EngineHost.SetFileExtension(extension);
        }

        public void SetOutputEncoding(Encoding encoding, bool fromOutputDirective)
        {
            EngineHost.SetOutputEncoding(encoding, fromOutputDirective);
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

            if (serviceType == typeof(ISubSonicCoreService))
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
                    result = await serviceProvider.GetServiceAsync(serviceType);
                });
            }

            return result;
        }

        public void Dispose()
        {
            if (templating is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
