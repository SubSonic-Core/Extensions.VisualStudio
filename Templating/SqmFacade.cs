using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Templating
{
    internal class SqmFacade
    {
        private static readonly Guid CommandSetIdentifier = new Guid("{ccd03feb-6b80-4cdb-ab3a-04702f6e7553}");
        private static DTE dte;

        public static void Initialize(DTE dte)
        {
            if (SqmFacade.dte == null)
            {
                SqmFacade.dte = dte;
            }
        }

        private static void RunCommand(Guid commandGuid, int cmdId)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            object obj2 = null;
            object obj3 = null;
            try
            {
                if (dte != null)
                {
                    dte.Commands.Raise(commandGuid.ToString("B").ToUpper(CultureInfo.InvariantCulture), cmdId, ref obj2, ref obj3);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void T4BeginSession()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2101);
        }

        public static void T4CompositionServices()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2113);
        }

        public static void T4MarkProjectForTextTemplating()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2110);
        }

        public static void T4PreprocessTemplateCS()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2105);
        }

        public static void T4PreprocessTemplateVB()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2104);
        }

        public static void T4ProcessHostSpecific()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2119);
        }

        public static void T4ProcessTemplateCS()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2103);
        }

        public static void T4ProcessTemplateVB()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2102);
        }

        public static void T4ResolveDirectiveProcessor()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2106);
        }

        public static void T4SetFileExtensionCS()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2108);
        }

        public static void T4SetFileExtensionOther()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2109);
        }

        public static void T4SetFileExtensionVB()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2107);
        }

        public static void T4TemplatedCodeGenerator()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2111);
        }

        public static void T4TemplatedPreprocessor()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2112);
        }

        public static void T4TransformAll101Plus()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2118);
        }

        public static void T4TransformAll11To20()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2116);
        }

        public static void T4TransformAll1To5()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2114);
        }

        public static void T4TransformAll21To100()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2117);
        }

        public static void T4TransformAll6To10()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RunCommand(CommandSetIdentifier, 0x2115);
        }
    }
}
