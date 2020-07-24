using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using SubSonic.Core.VisualStudio.CustomTools;
using SubSonic.Core.VisualStudio.Services;
using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace SubSonic.Core.VisualStudio
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideService((typeof(SSubSonicTemplatingService)), IsAsyncQueryable = true)]
    [ProvideCodeGenerator(typeof(SubSonicTemplatingFileGenerator), nameof(SubSonicTemplatingFileGenerator), "Generator that uses the Text Templating engine with vsix.", true, ProjectSystem = "{fae04ec1-301f-11d3-bf4b-00c04f79efbc}")]
    [ProvideCodeGenerator(typeof(SubSonicTemplatingFileGenerator), nameof(SubSonicTemplatingFileGenerator), "Generator that uses the Text Templating engine with vsix.", true, ProjectSystem = "{164b10b9-b200-11d0-8c61-00a0c91e29d5}")]
    [ProvideCodeGenerator(typeof(SubSonicTemplatingFileGenerator), nameof(SubSonicTemplatingFileGenerator), "Generator that uses the Text Templating engine with vsix.", true, ProjectSystem = "{39c9c826-8ef8-4079-8c95-428f5b1c323f}")]
    [ProvideCodeGeneratorExtension(nameof(SubSonicTemplatingFileGenerator), ".stt", ProjectSystem = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}", ProjectSystemPackage = "{fae04ec1-301f-11d3-bf4b-00c04f79efbc}")]
    [ProvideCodeGeneratorExtension(nameof(SubSonicTemplatingFileGenerator), ".stt", ProjectSystem = "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}", ProjectSystemPackage = "{164b10b9-b200-11d0-8c61-00a0c91e29d5}")]
    [ProvideCodeGeneratorExtension(nameof(SubSonicTemplatingFileGenerator), ".stt", ProjectSystem = "{E24C65DC-7377-472b-9ABA-BC803B73C61A}", ProjectSystemPackage = "{39c9c826-8ef8-4079-8c95-428f5b1c323f}")]
    [Guid(PackageGuidString)]
    public sealed class SubSonicCoreVisualStudioAsyncPackage : AsyncPackage
    {
        /// <summary>
        /// SubSonic.Core.VisualStudioPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "fb84c14b-ef14-43ce-810a-1f6d1b3fbefb";

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            AsyncServiceCreatorCallback callback = new AsyncServiceCreatorCallback(CreateServiceAsync);

            AddService(typeof(SSubSonicTemplatingService), CreateServiceAsync, true);
        }
        
        public async Task<object> CreateServiceAsync(IAsyncServiceContainer container, CancellationToken cancellationToken, Type serviceType)
        {
            if (serviceType == typeof(SSubSonicTemplatingService))
            {
                return await new SubSonicTemplatingService(this).InitializeAsync(cancellationToken);
            }

            return null;
        }

        #endregion
    }
}
