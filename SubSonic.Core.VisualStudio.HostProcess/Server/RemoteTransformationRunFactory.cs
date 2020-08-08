using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.VisualStudio.HostProcess.Server
{
    public sealed class RemoteTransformationRunFactory
        : TransformationRunFactory
    {
        public RemoteTransformationRunFactory(Guid id)
            : base(id) { }

        public override IProcessTransformationRunner CreateTransformationRunner(Type runnerType)
        {
            Type remoteType = typeof(RemoteTransformationRunner);

            if (runnerType.IsAssignableFrom(remoteType))
            {
                return base.CreateTransformationRunner(remoteType);
            }

            return base.CreateTransformationRunner(runnerType);
        }
    }
}
