using Microsoft.VisualStudio.Shell;
using Mono.TextTemplating.CodeCompilation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Package = SubSonic.Core.VisualStudio.SubSonicCoreVisualStudioAsyncPackage;

namespace SubSonic.Core.VisualStudio.Forms
{
    [ProvideProfile(typeof(TemplatingHostOptionsPageGrid), Package.SubSonicCoreCategory, HostOptionsPageName, 100, 101, true)]
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
        [Description("Additional Roslyn compiler options.")]
        public string CompilerOptions
        {
            get => compilerOptions;
            set => compilerOptions = value;
        }

        [Category(Package.SubSonicCoreCategory)]
        [DisplayName("Roslyn Runtime")]
        [Description("Indicates which Roslyn runtime to use.")]
        public RuntimeKind RuntimeKind { get; } = RuntimeKind.NetCore;
        //{
        //    get => runtimeKind;
        //    set => runtimeKind = value;
        //}

        [Category(Package.SubSonicCoreCategory)]
        [DisplayName("Host Execution Timeout")]
        [Description("Length of time the host will wait for activity before shutting down, in seconds.")]
        public int Timeout { get; } = 120;

        private bool linePragmas = true;

        [Category(Package.SubSonicCoreCategory)]
        [DisplayName("Line Pragmas")]
        [Description("Generate line pragmas.")]
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
