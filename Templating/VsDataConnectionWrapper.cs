using Microsoft.VisualStudio.Data.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Templating
{
    internal class VsDataConnectionWrapper
        : IDataConnection
    {
        private readonly IVsDataConnection Connection;

        public VsDataConnectionWrapper(IVsDataConnection connection)
        {
            Connection = connection;
        }

        public string EncryptedConnectionString => Connection.EncryptedConnectionString;

        public string SafeConnectionString => Connection.SafeConnectionString;

        public string DisplayConnectionString => Connection.DisplayConnectionString;
    }
}
