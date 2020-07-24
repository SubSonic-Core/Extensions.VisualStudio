using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio
{
    public interface IDataConnection
    {
        string EncryptedConnectionString { get; }
        string SafeConnectionString { get; }
        string DisplayConnectionString { get; }
    }
}
