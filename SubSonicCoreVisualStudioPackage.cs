using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using SubSonic.Core.VisualStudio.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio
{
    [ProvideService(typeof(SubSonicCoreService))]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(PackageGuidString)]
    public sealed class SubSonicCoreVisualStudioPackage 
        : Package
    {
        /// <summary>
        /// SubSonic.Core.VisualStudioPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "828778C3-002E-47D5-8582-790B65F8A334";

        protected override void Initialize()
        {
            ServiceCreatorCallback callback = new ServiceCreatorCallback(CreateService);

            if (this is IServiceContainer container)
            {
                container.AddService(typeof(SSubSonicCoreService), callback);
            }

            base.Initialize();
        }

        private object CreateService(IServiceContainer container, Type serviceType)
        {
            if (typeof(SSubSonicCoreService) == serviceType)
            {
                return new SubSonicCoreService(this, container.GetService<STextTemplating, ITextTemplatingEngineHost>());
            }

            return null;
        }
    }
}
