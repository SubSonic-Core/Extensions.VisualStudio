#if !NETFRAMEWORK
using System;
using System.Reflection;
using System.Runtime.Loader;

namespace SubSonic.Core.VisualStudio.Host
{
    [Serializable]
    public class RemoteAssemblyLoadContext
        : AssemblyLoadContext
    {
#if NETCOREAPP || NET5
        public RemoteAssemblyLoadContext()
            : base(true) { }
#else
        public virtual void Unload()
        {
            throw new NotSupportedException("Feature not supported by runtime.");
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
#endif