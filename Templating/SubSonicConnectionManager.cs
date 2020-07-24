using Microsoft.VisualStudio.Data.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Templating
{
    [Serializable]
    internal class SubSonicConnectionManager
        : IConnectionManager
    {
        private readonly IDictionary<string, IDataConnection> ConnectionManager;

        public SubSonicConnectionManager()
        {
            ConnectionManager = new Dictionary<string, IDataConnection>();
        }

        public IDataConnection this[string key]
        {
            get
            {
                if (ConnectionManager.ContainsKey(key))
                {
                    return ConnectionManager[key];
                }
                else
                {
                    throw new InvalidOperationException(string.Format(SubSonicCoreErrors.MissingConnectionKey, key));
                }
            }
        }

        public void Add(string key, IDataConnection connection)
        {
            if (!ConnectionManager.ContainsKey(key))
            {
                ConnectionManager.Add(key, connection);
            }
            else
            {
                ConnectionManager[key] = connection;
            }
        }
    }
}
