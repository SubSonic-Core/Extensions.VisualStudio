using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Microsoft.VisualStudio.Threading;
using SubSonic.Core.VisualStudio.Templating;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.CustomTools
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration(nameof(SubSonicTemplatingFileGenerator), "Expose additional visual studio services to the Text Templating Engine.", "1.0")]
    [Guid("012F51A1-8653-4C08-BCB8-02CD7025FFA9")]
    [ProvideObject(typeof(SubSonicTemplatingFileGenerator))]
    [CodeGeneratorRegistration(typeof(SubSonicTemplatingFileGenerator), "Generator that uses the Text Templating engine with vsix.", "{00D1A9C2-B5F0-4AF3-8072-F6C62B433612}", GeneratesDesignTimeSource = true)]
    [CodeGeneratorRegistration(typeof(SubSonicTemplatingFileGenerator), "Generator that uses the Text Templating engine with vsix.", "{13B669BE-BB05-4DDF-9536-439F39A36129}", GeneratesDesignTimeSource = true)]
    [CodeGeneratorRegistration(typeof(SubSonicTemplatingFileGenerator), "Generator that uses the Text Templating engine with vsix.", "{164B10B9-B200-11D0-8C61-00A0C91E29D5}", GeneratesDesignTimeSource = true)]
    [CodeGeneratorRegistration(typeof(SubSonicTemplatingFileGenerator), "Generator that uses the Text Templating engine with vsix.", "{39C9C826-8EF8-4079-8C95-428F5B1C323F}", GeneratesDesignTimeSource = true)]
    [CodeGeneratorRegistration(typeof(SubSonicTemplatingFileGenerator), "Generator that uses the Text Templating engine with vsix.", "{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}", GeneratesDesignTimeSource = true)]
    public partial class SubSonicTemplatingFileGenerator
        : TemplatedCodeGenerator
    {
        public SubSonicTemplatingFileGenerator()
            : base()
        {
        }

        protected override string ProcessTemplate(string inputFileName, string inputFileContent, ITextTemplating processor, IVsHierarchy hierarchy)
        {
            // we are replacing the processor with our subsonic enhanced processor
            ThreadHelper.JoinableTaskFactory.Run(async delegate {
                processor = await GetServiceAsync<SSubSonicCoreService, ITextTemplating>();
                });

            processor.BeginErrorSession();

            string result = base.ProcessTemplate(inputFileName, inputFileContent, processor, hierarchy);

            callback.Errors |= processor.EndErrorSession();

            return result;
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
