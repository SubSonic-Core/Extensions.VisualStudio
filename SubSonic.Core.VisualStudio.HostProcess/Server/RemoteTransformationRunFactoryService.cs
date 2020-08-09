using Mono.VisualStudio.TextTemplating;
using SubSonic.Core.VisualStudio.Host;
using System;

namespace SubSonic.Core.VisualStudio.HostProcess.Server
{
    [Serializable]
    public sealed class RemoteTransformationRunFactoryService
        : TransformationRunFactoryService
    {
        public RemoteTransformationRunFactoryService(Uri serviceUri)
            : base(serviceUri) { }

        public override IProcessTransformationRunFactory TransformationRunFactory(Guid id)
        {
            IProcessTransformationRunFactory factory = new RemoteTransformationRunFactory(id, new RemoteAssemblyLoadContext())
            {
                IsAlive = true
            };

            if (runFactories.TryAdd(id, factory))
            {
                return factory;
            }

            return default;
        }
    }
}
