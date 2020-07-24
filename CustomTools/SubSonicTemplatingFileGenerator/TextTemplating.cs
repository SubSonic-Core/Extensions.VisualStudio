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
    [InstalledProductRegistration(nameof(SubSonicTemplatingFileGenerator), "Expose additional visual studio services to the Text Templating Engine.", "1.0")]
    [Guid("012F51A1-8653-4C08-BCB8-02CD7025FFA9")]
    public partial class SubSonicTemplatingFileGenerator
        : TemplatedCodeGenerator
    {
        public SubSonicTemplatingFileGenerator()
            : base()
        {
        }

        public override string GetDefaultExtension()
        {
            return this.callback.Extension;
        }

        protected override string ProcessTemplate(string inputFileName, string inputFileContent, ITextTemplating processor, IVsHierarchy hierarchy)
        {
            // we are replacing the processor with our subsonic enhanced processor
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                processor = await GetServiceAsync<SSubSonicTemplatingService, ITextTemplating>();
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
