using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Package = SubSonic.Core.VisualStudio.SubSonicCoreVisualStudioAsyncPackage;

namespace SubSonic.Core.VisualStudio.Forms
{
    [ProvideProfile(typeof(SecurityOptionPageGrid), Package.SubSonicCoreCategory, HostOptionsPageName, 100, 101, true)]
    public class TemplatingHostOptionsPageGrid
        : DialogPage
    {
        public const string HostOptionsPageName = "Host Options";

        private bool cachedTemplates = true;

        [Category(Package.SubSonicCoreCategory)]
        [DisplayName("Use Cached Templates")]
        [Description("Cache Compiled Templates.")]
        public bool CachedTemplates
        {
            get => cachedTemplates;
            set => cachedTemplates = value;
        }

        private string compilerOptions = "";

        [Category(Package.SubSonicCoreCategory)]
        [DisplayName("Compiler Options")]
        [Description("TODO:: Set Description")]
        public string CompilerOptions
        {
            get => compilerOptions;
            set => compilerOptions = value;
        }

        private bool linePragmas = true;

        [Category(Package.SubSonicCoreCategory)]
        [DisplayName("Line Pragmas")]
        [Description("TODO:: Set Description")]
        public bool LinePragmas
        {
            get => linePragmas;
            set => linePragmas = value;
        }

        private bool relativeLinePragmas = true;

        [Category(Package.SubSonicCoreCategory)]
        [DisplayName("Relative Line Pragmas")]
        [Description("TODO:: Set Description")]
        public bool UseRelativeLinePragmas
        {
            get => relativeLinePragmas;
            set => relativeLinePragmas = value;
        }
    }
}
