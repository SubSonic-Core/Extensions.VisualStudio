using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Package = SubSonic.Core.VisualStudio.SubSonicCoreVisualStudioAsyncPackage;

namespace SubSonic.Core.VisualStudio.Forms
{
    //[Guid("A36B1D29-BF91-44C4-AF24-BF2ADC08C227")]
    [ProvideProfile(typeof(SecurityOptionPageGrid), Package.SubSonicCoreCategory, SecurityPageName, 100, 101, true)]
    public class SecurityOptionPageGrid
        : DialogPage
    {
        public const string SecurityPageName = "Security";

        private bool showSecurityWarning = true;

        [Category(Package.SubSonicCoreCategory)]
        [DisplayName("Show Security Warning")]
        [Description("Warn about the dangers of executing untrusted scripts.")]
        public bool ShowSecurityWarning
        {
            get => showSecurityWarning;
            set => showSecurityWarning = value;
        }
    }
}
