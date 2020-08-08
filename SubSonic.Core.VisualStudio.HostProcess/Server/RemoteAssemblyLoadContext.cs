using System.Runtime.Loader;

namespace SubSonic.Core.VisualStudio.HostProcess.Server
{
    internal class RemoteAssemblyLoadContext
        : AssemblyLoadContext
    {
        public RemoteAssemblyLoadContext()
            : base(true) { }
    }
}
