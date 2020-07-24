using Microsoft.VisualStudio.Data.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Forms
{
    public partial class DataContextForm
    {
        private readonly IVsDataExplorerConnectionManager connectionManager;

        private IList ListOfConnections
        {
            get
            {
                if (connectionManager != null && connectionManager.Connections != null)
                {
                    return connectionManager.Connections.Keys.ToList();
                }

                return new List<string>();
            }
        }

        public string SelectedConnectionName => connections.SelectedItem.ToString();

        private void BindDataSources()
        {
            this.connections.DataSource = ListOfConnections;
        }
    }
}
