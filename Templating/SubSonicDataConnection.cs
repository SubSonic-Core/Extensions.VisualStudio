using Microsoft.VisualStudio.Data.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Templating
{
    [Serializable]
    internal class SubSonicDataConnection
        : IDataConnection
    {
        public SubSonicDataConnection(IVsDataConnection connection)
        {
            EncryptedConnectionString = connection.EncryptedConnectionString;
            SafeConnectionString = connection.SafeConnectionString;
            DisplayConnectionString = connection.DisplayConnectionString;
        }

        public string EncryptedConnectionString { get; set; }

        public string SafeConnectionString { get; set; }

        public string DisplayConnectionString { get; set; }
    }
}
