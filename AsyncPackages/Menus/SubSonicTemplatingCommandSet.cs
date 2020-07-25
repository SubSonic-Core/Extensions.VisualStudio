using EnvDTE;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.Telemetry;
using SubSonic.Core.VisualStudio.CustomTools;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.VisualStudio.VSConstants;
using Task = System.Threading.Tasks.Task;

namespace SubSonic.Core.VisualStudio.AsyncPackages.Menus
{
    [Guid("96D631CC-5C00-40C0-A186-B1A6702C8F84")]
    public sealed class SubSonicTemplatingCommandSet
    {
        //private static OutputWindowPane outputWindowPane;
        private IMenuCommandService menuService;
        private SubSonicCoreVisualStudioAsyncPackage package;

        public SubSonicTemplatingCommandSet(SubSonicCoreVisualStudioAsyncPackage package)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));

            MenuCommand[] menuCommands = new MenuCommand[]
            {
                new OleMenuCommand(OnMenuDebugTemplate, null, OnStatusDebugTemplate, SubSonicCommandIDS.DebugTemplate, SubSonicMenuCommands.DebugTemplate)
            };

            if (package.GetService<IMenuCommandService>() is IMenuCommandService service)
            {
                menuService = service;

                foreach (MenuCommand command in menuCommands)
                {
                    menuService.AddCommand(command);
                }
            }
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

                        if (package != null && SaveAllOpenFiles())
                        {
                            package.DebugTemplate(projectItem, hierarchy, fileName, File.ReadAllText(fileName));
                        }
                    }
                }
            }
        }

        private bool SaveAllOpenFiles()
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (package.GetService<SVsSolutionBuildManager>() is IVsSolutionBuildManager2 service)
                {
                    ErrorHandler.ThrowOnFailure(service.SaveDocumentsBeforeBuild(null, (uint)VSITEMID.Nil, 0));
                }
            }
            catch (COMException ex)
            {
                if ((ex.ErrorCode != -2147467260) && (ex.ErrorCode != -2147221492))
                {
                    throw;
                }
                return false;
            }
            return true;
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
                            command.Enabled = !package.IsDebuggingTemplate;
                        }
                    }
                }
                catch(Exception)
                {
                    command.Visible = false;
                }
            }
        }

        private string GetCustomTool(ProjectItem projectItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (projectItem.Properties != null)
            {
                try
                {
                    if (projectItem.Properties.Item("CustomTool") is Property property)
                    {
                        string result = property.Value as string;

                        if (result.IsNotNullOrEmpty())
                        {
                            return result;
                        }
                    }
                }
                catch(ArgumentException)
                {

                }
            }
            return null;
        }

        private bool TemplatingErrorStatus => package?.TextTemplatingServiceErrorStatus ?? default;

        private void ClearCustomToolTemplatingErrorStatus()
        {
            package?.ClearTemplatingErrorStatus();
        }
    }
}
