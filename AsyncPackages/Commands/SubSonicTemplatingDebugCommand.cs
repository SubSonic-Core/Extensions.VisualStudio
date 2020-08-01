using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using SubSonic.Core.VisualStudio.CustomTools;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using Task = System.Threading.Tasks.Task;

namespace SubSonic.Core.VisualStudio.AsyncPackages.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SubSonicTemplatingDebugCommand
        : BaseSubSonicCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("d1e1f06e-3219-46fe-9b29-960abba35c57");

        public static readonly Guid SolutionCommandSet = new Guid("1496A755-94DE-11DE-8C3F-00C04FC2AAE2");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        protected override SubSonicCoreVisualStudioAsyncPackage SubSonicPackage => package as SubSonicCoreVisualStudioAsyncPackage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubSonicTemplatingDebugCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private SubSonicTemplatingDebugCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var toolMenuCommandID = new CommandID(CommandSet, CommandId);
            var toolMenuItem = new OleMenuCommand(OnMenuDebugTemplate, null, OnStatusDebugTemplate, toolMenuCommandID, SubSonicMenuCommands.DebugTemplate);
            commandService.AddCommand(toolMenuItem);
            var solutionContextMenuCommandID = new CommandID(SolutionCommandSet, CommandId);
            var solutionContextMenuItem = new OleMenuCommand(OnMenuDebugTemplate, null, OnStatusDebugTemplate, solutionContextMenuCommandID, SubSonicMenuCommands.DebugTemplate);
            commandService.AddCommand(solutionContextMenuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static SubSonicTemplatingDebugCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in SubSonicTemplatingCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SubSonicTemplatingDebugCommand(package, commandService);
        }
                
        private void OnMenuDebugTemplate(object sender, EventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (sender is OleMenuCommand command)
            {
                IntPtr hieracrhyPtr;
                IntPtr selectionContainerPtr;
                uint num;
                IVsMultiItemSelect select;

                ErrorHandler.ThrowOnFailure(((IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection))).GetCurrentSelection(out hieracrhyPtr, out num, out select, out selectionContainerPtr));

                if (Marshal.GetTypedObjectForIUnknown(hieracrhyPtr, typeof(IVsHierarchy)) is IVsHierarchy hierarchy)
                {
                    object extObject;
                    ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(num, (int)__VSHPROPID.VSHPROPID_ExtObject, out extObject));

                    if (extObject is ProjectItem projectItem)
                    {
                        string fileName = projectItem.FileNames[1];

                        if (SubSonicPackage != null && SaveAllOpenFiles())
                        {
                            SubSonicPackage.ProcessDebugTemplate(projectItem, hierarchy, fileName, File.ReadAllText(fileName));
                        }
                    }
                }
            }
        }

        private void OnStatusDebugTemplate(object sender, EventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (sender is OleMenuCommand command)
            {
                try
                {
                    IntPtr hieracrhyPtr;
                    IntPtr selectionContainerPtr;
                    uint num;
                    IVsMultiItemSelect select;

                    ErrorHandler.ThrowOnFailure(((IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection))).GetCurrentSelection(out hieracrhyPtr, out num, out select, out selectionContainerPtr));

                    if (Marshal.GetTypedObjectForIUnknown(hieracrhyPtr, typeof(IVsHierarchy)) is IVsHierarchy hierarchy)
                    {
                        object extObject;
                        ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(num, (int)__VSHPROPID.VSHPROPID_ExtObject, out extObject));

                        if (extObject is ProjectItem projectItem)
                        {
                            command.Visible = GetCustomTool(projectItem).Equals(nameof(SubSonicTemplatingFileGenerator), StringComparison.OrdinalIgnoreCase);
                            command.Enabled = !SubSonicPackage.IsTemplateInProcess;
                        }
                    }
                }
                catch (Exception)
                {
                    command.Visible = false;
                    command.Enabled = false;
                }
            }
        }


    }
}
