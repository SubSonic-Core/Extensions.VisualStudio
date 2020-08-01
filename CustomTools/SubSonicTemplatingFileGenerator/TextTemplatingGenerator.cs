using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Mono.TextTemplating;
using Mono.TextTemplating.CodeCompilation;
using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using SubSonic.Core.VisualStudio.Common;
using SubSonic.Core.VisualStudio.Templating;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using BaseCodeGeneratorWithSite = Microsoft.VisualStudio.TextTemplating.VSHost.BaseCodeGeneratorWithSite;

namespace SubSonic.Core.VisualStudio.CustomTools
{
    [InstalledProductRegistration(nameof(SubSonicTemplatingFileGenerator), "Expose additional visual studio services to the Text Templating Engine.", "1.0")]
    [Guid("012F51A1-8653-4C08-BCB8-02CD7025FFA9")]
    public partial class SubSonicTemplatingFileGenerator
        : BaseCodeGeneratorWithSite
    {
        private const string namespace_hint = "NamespaceHint";

        private const string TEMPLATE_NAMESPACE = "SubSonic.Core.Templating";
        private const string TEMPLATE_CLASS = "SubSonicTemplate";
        private const string ERROR_OUTPUT = "ErrorGeneratingOutput";

        protected ITextTemplatingCallback callback = new TextTemplatingCallback();

        SubSonicCoreVisualStudioAsyncPackage Package => SubSonicCoreVisualStudioAsyncPackage.Singleton;

        public SubSonicTemplatingFileGenerator()
            : base()
        {
            
        }

        public override string GetDefaultExtension()
        {
            return this.callback.Extension;
        }

        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            string result = "";

            ThreadHelper.ThrowIfNotOnUIThread();

            if ((Package?.IsTemplateInProcess ?? false) && Package?.ProcessResults != null)
            {
                callback = Package.TextTemplatingCallback.DeepCopy();
                result = Package.ProcessResults;
                Package.ProcessResults = null;
            }
            else
            {
                if (!Package.ShowSecurityWarningDialog())
                {
                    return Array.Empty<byte>();
                }
                base.SetWaitCursor();
                callback = new TextTemplatingCallback();
                ITextTemplating processor = TextTemplating;
                CallContext.LogicalSetData(namespace_hint, base.FileNamespace);

                try
                {
                    if (processor == null)
                    {
                        throw new InvalidOperationException(SubSonicCoreErrors.TextTemplatingUnavailable);
                    }

                    processor.BeginErrorSession();

                    if (GetService(typeof(IVsHierarchy)) is IVsHierarchy hierarchy)
                    {
                        result = ProcessTemplate(inputFileName, inputFileContent, processor, hierarchy);

                        callback.Errors |= processor.EndErrorSession();
                        MarkProjectForTextTemplating(hierarchy);
                    }
                }
                finally
                {
                    CallContext.FreeNamedDataSlot(namespace_hint);
                }
            }

            if (callback.Errors)
            {
                if (ErrorList is IVsErrorList vsErrorList)
                {
                    try
                    {
                        vsErrorList.BringToFront();
                        vsErrorList.ForceShowErrors();
                    }
                    catch
                    { }
                }
            }

            callback.SetOutputEncoding(EncodingHelper.GetEncoding(inputFileName) ?? Encoding.UTF8, false);

            result = result?.TrimStart(Environment.NewLine.ToCharArray()) ?? ERROR_OUTPUT;

            byte[] bytes = callback.OutputEncoding.GetBytes(result);
            byte[] preamble = callback.OutputEncoding.GetPreamble();
            if ((preamble != null) && (preamble.Length != 0))
            {
                bool success = false;
                if (bytes.Length >= preamble.Length)
                {
                    success = true;
                    for (int i = 0; i < preamble.Length; i++)
                    {
                        if (preamble[i] != bytes[i])
                        {
                            success = false;
                            break;
                        }
                    }
                }
                if (!success)
                {
                    byte[] array = new byte[preamble.Length + bytes.Length];
                    preamble.CopyTo(array, 0);
                    bytes.CopyTo(array, preamble.Length);
                    bytes = array;
                }
            }

            SqmFacade.T4TemplatedCodeGenerator();

            return bytes;
        }

        protected virtual ITextTemplating TextTemplating
        {
            get
            {
                ITextTemplating processor = null;

                ThreadHelper.JoinableTaskFactory.Run(async delegate
                {
                    processor = await GetServiceAsync<SSubSonicTemplatingService, ITextTemplating>();
                });

                if (processor is ITextTemplatingEngineHost host)
                {
                    host.StandardAssemblyReferences.AddIfNotExist("System.Data");
                    host.StandardAssemblyReferences.AddIfNotExist("System.Data.Common");
                    host.StandardAssemblyReferences.AddIfNotExist("System.ComponentModel");
                    host.StandardAssemblyReferences.AddIfNotExist("SubSonic.Core.DataAccessLayer");

                    host.StandardImports.AddIfNotExist("SubSonic");
                }

                return processor;
            }
        }

        private void MarkProjectForTextTemplating(IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Guid gUID = typeof(SSubSonicTemplatingService).GUID;
            IVsProjectStartupServices startUpServices = GetStartUpServices(hierarchy);

            if ((startUpServices != null) && !StartupServicesReferencesService(startUpServices, gUID))
            {
                ErrorHandler.Failed(startUpServices.AddStartupService(ref gUID));
                SqmFacade.T4MarkProjectForTextTemplating();
            }
        }

        private IVsProjectStartupServices GetStartUpServices(IVsHierarchy hierarchy)
        {
            object obj2;

            ThreadHelper.ThrowIfNotOnUIThread();

            if (ErrorHandler.Succeeded(hierarchy.GetProperty(0xfffffffe, -2040, out obj2)))
            {
                return obj2 as IVsProjectStartupServices;
            }

            return default;
        }

        private bool StartupServicesReferencesService(IVsProjectStartupServices startup, Guid serviceId)
        {
            bool success = false;
            IEnumProjectStartupServices services = null;

            ThreadHelper.ThrowIfNotOnUIThread();

            ErrorHandler.ThrowOnFailure(startup.GetStartupServiceEnum(out services));
            uint num = 0;
            Guid[] guidArray = new Guid[1];
            while (true)
            {
                int hr = services.Next(1, guidArray, out num);
                ErrorHandler.ThrowOnFailure(hr);
                if ((num == 1) && (guidArray[0].CompareTo(serviceId) == 0))
                {
                    success = true;
                }
                else if (hr != 1)
                {
                    continue;
                }
                return success;
            }
        }

        protected virtual string ProcessTemplate(string inputFileName, string inputFileContent, ITextTemplating processor, IVsHierarchy hierarchy)
        {
            string content = null;

            if (processor is ITextTemplatingService service)
            {
                RuntimeKind runtime = (RuntimeKind)(service.GetHostOption(nameof(TemplateSettings.RuntimeKind)) ?? RuntimeKind.Default);

                if (runtime != RuntimeKind.NetCore)
                {
                    content = processor.ProcessTemplate(inputFileName, inputFileContent, callback, hierarchy);
                }
                else
                {
                    ThreadHelper.JoinableTaskFactory.Run(async () =>
                    {
                        if (service is IProcessTextTemplating process)
                        {
                            content = await process.ProcessTemplateAsync(inputFileName, inputFileContent, callback, hierarchy);
                        }
                    });
                }
            }
            return content;
        }

        public async Task<TInterface> GetServiceAsync<TService, TInterface>()
        {
            if (await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(TService)) is TInterface success)
            {
                return success;
            }

            return default;
        }
    }
}
