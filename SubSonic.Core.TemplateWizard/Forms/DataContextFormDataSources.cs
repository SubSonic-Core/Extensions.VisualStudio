using Microsoft.VisualStudio.Data.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.Template.Forms
{
    public partial class DataContextForm
    {
        private readonly IVsDataExplorerConnectionManager connectionManager;

        private IList ListOfConnections => connectionManager.Connections.Keys.ToList();

        public string SelectedConnectionName => connections.SelectedItem.ToString();

        private void BindDataSources()
        {
            this.connections.DataSource = ListOfConnections;
        }
    }
}
