using Mono.VisualStudio.TextTemplating.VSHost;
using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Text;

namespace SubSonic.Core.VisualStudio.HostProcess.Server
{
    [Serializable]
    public class RemoteTransformationRunner
        : TransformationRunner
    {
        public RemoteTransformationRunner(TransformationRunFactory factory, Guid runnerId)
            : base(factory, runnerId) { }

        protected override AssemblyLoadContext GetLoadContext()
        {
            return new RemoteAssemblyLoadContext();
        }

        protected override void Unload(AssemblyLoadContext context)
        {
            context.Unload();
        }
    }
}
