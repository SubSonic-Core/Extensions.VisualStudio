using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace SubSonic.Core.VisualStudio.CustomTools
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration(nameof(SubSonicTemplatingFileGenerator), "Expose additional visual studio services to the Text Templating Engine.", "1.0")]
    [Guid("012F51A1-8653-4C08-BCB8-02CD7025FFA9")]
    [ComVisible(true)]
    [ProvideObject(typeof(SubSonicTemplatingFileGenerator))]
    [CodeGeneratorRegistration(typeof(SubSonicTemplatingFileGenerator), nameof(SubSonicTemplatingFileGenerator), "{00D1A9C2-B5F0-4AF3-8072-F6C62B433612}", GeneratesDesignTimeSource = true)]
    public partial class SubSonicTemplatingFileGenerator
        : TemplatedCodeGenerator
        , IVsSingleFileGenerator
    {
        public int DefaultExtension(out string pbstrDefaultExtension)
        {
            pbstrDefaultExtension = ".cs";

            return pbstrDefaultExtension.Length;
        }
    }
}
