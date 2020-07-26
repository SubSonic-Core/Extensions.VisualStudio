using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using static Microsoft.VisualStudio.VSConstants;

namespace SubSonic.Core.VisualStudio.AsyncPackages.Commands
{
    public abstract class BaseSubSonicCommand
    {
        protected abstract SubSonicCoreVisualStudioAsyncPackage SubSonicPackage { get; }

        protected bool SaveAllOpenFiles()
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (SubSonicPackage.GetService<SVsSolutionBuildManager>() is IVsSolutionBuildManager2 service)
                {
                    ErrorHandler.ThrowOnFailure(service.SaveDocumentsBeforeBuild(null, (uint)0xfffffffe, 0));
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

        protected string GetCustomTool(ProjectItem projectItem)
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
                catch (ArgumentException)
                {

                }
            }
            return "";
        }

        protected bool TemplatingErrorStatus => SubSonicPackage?.TextTemplatingServiceErrorStatus ?? default;

        protected void ClearCustomToolTemplatingErrorStatus()
        {
            SubSonicPackage?.ClearTemplatingErrorStatus();
        }
    }
}
