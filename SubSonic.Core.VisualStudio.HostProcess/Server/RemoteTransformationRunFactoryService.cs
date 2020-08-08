using Mono.VisualStudio.TextTemplating;
using SubSonic.Core.VisualStudio.Host;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.VisualStudio.HostProcess.Server
{
    public sealed class RemoteTransformationRunFactoryService
        : TransformationRunFactoryService
    {
        public RemoteTransformationRunFactoryService(Uri serviceUri)
            : base(serviceUri) { }

        public override IProcessTransformationRunFactory TransformationRunFactory(Guid id)
        {
            IProcessTransformationRunFactory factory = new RemoteTransformationRunFactory(id)
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
