using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio
{
    public interface IConnectionManager
    {
        IDataConnection this[string key] { get; }
    }
}
