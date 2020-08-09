using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Host
{
    public class RemoteAssemblyLoadContext
        : AssemblyLoadContext
    {
#if NETCOREAPP || NET5
        public RemoteAssemblyLoadContext()
            : base(true) { }
#else
        public virtual void Unload()
        {
            throw new PlatformNotSupportedException("Feature not supported by runtime.");
        }

#if NETSTANDARD || NETFRAMEWORK
        protected override Assembly Load(AssemblyName assemblyName)
        {
            return Assembly.Load(assemblyName);
        }
#endif
#endif
    }
}