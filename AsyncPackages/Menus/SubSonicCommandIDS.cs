using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.AsyncPackages.Menus
{
    public static class SubSonicCommandIDS
    {
        public const int CmdIDForDebugTemplate = 0x1001;
        public static readonly CommandID DebugTemplate = new CommandID(CommandIds.GuidOrchestratorSolutionExplorerCmdSet, CmdIDForDebugTemplate);
    }
}
