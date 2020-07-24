using Microsoft.VisualStudio.Data.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Templating
{
    internal class VsDataExplorerConnectionWrapper
        : IConnectionManager
    {
        private readonly IVsDataExplorerConnectionManager ConnectionManager;
        public VsDataExplorerConnectionWrapper(IVsDataExplorerConnectionManager connectionManager)
        {
            ConnectionManager = connectionManager;
        }

        public IDataConnection this[string key]
        {
            get
            {
                if (ConnectionManager.Connections.ContainsKey(key))
                {
                    return new VsDataConnectionWrapper(ConnectionManager.Connections[key].Connection);
                }
                else
                {
                    throw new InvalidOperationException(string.Format(SubSonicCoreErrors.MissingConnectionKey, key));
                }
            }
        }
    }
}
