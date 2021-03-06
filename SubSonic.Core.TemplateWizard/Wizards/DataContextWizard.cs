﻿using EnvDTE;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Interop = Microsoft.VisualStudio.OLE.Interop;

namespace SubSonic.Core.VisualStudio.Wizards
{
    using SubSonic.Core.VisualStudio.Common;
    using SubSonic.Core.VisualStudio.Forms;

    public class DataContextWizard
        : IWizard
    {
        Interop.IServiceProvider Provider { get; set; }

        private DataContextForm inputForm;

        private bool shouldAddItems;

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
            
        }

        public void ProjectFinishedGenerating(Project project)
        {
            
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            
        }

        public void RunFinished()
        {
            
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            try
            {
                if (automationObject is Interop.IServiceProvider provider)
                {
                    Provider = provider;
                }

                if (Provider.QueryService<IVsDataExplorerConnectionManager>() is IVsDataExplorerConnectionManager connectionManager)
                {
                    // Display a form to the user. The form collects
                    // input for the custom message.
                    inputForm = new DataContextForm(connectionManager);

                    if (inputForm.ShowDialog() == DialogResult.OK)
                    {
                        shouldAddItems = true;

                        if (string.IsNullOrEmpty(inputForm.SelectedConnectionName) == false &&
                            connectionManager.Connections[inputForm.SelectedConnectionName] != null)
                        {
                            string connectionKey = inputForm.SelectedConnectionName;

                            replacementsDictionary.Add("$connectionKey$", connectionKey.ToConnectionKey());
                        }
                        else
                        {
                            throw new InvalidOperationException("Missing Connection Key");
                        }
                    }

                    //customMessage = DataContextForm.CustomMessage;

                    //// Add custom parameters.
                    //replacementsDictionary.Add("$custommessage$",
                    //    customMessage);
                }
            }
            catch (Exception ex)
            {
                shouldAddItems = false;

                MessageBox.Show(ex.ToString());
            }
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return shouldAddItems;
        }
    }
}
