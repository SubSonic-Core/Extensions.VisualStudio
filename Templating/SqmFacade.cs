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
            SqmFacade.dte = dte;
        }

        private static void RunCommand(Guid commandGuid, int cmdId)
        {
            object obj2 = null;
            object obj3 = null;
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (dte != null)
                {
                    dte.Commands.Raise(commandGuid.ToString("B").ToUpper(CultureInfo.InvariantCulture), cmdId, ref obj2, ref obj3);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void T4PreprocessTemplateCS()
        {
            RunCommand(CommandSetIdentifier, 0x2105);
        }

        public static void T4PreprocessTemplateVB()
        {
            RunCommand(CommandSetIdentifier, 0x2104);
        }

        public static void T4TemplatedCodeGenerator()
        {
            RunCommand(CommandSetIdentifier, 0x2111);
        }
    }
}
