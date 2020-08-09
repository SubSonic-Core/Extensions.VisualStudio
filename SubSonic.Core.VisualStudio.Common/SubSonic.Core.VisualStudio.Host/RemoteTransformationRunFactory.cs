using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Host
{
    [Serializable]
    public class RemoteTransformationRunFactory
        : TransformationRunFactory
    {
        [NonSerialized]
        private readonly AssemblyLoadContext context;

        public RemoteTransformationRunFactory(Guid id, AssemblyLoadContext context)
            : base(id)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override IProcessTransformationRunner CreateTransformationRunner(Type runnerType)
        {
            var runnerId = Guid.NewGuid();

            if (Activator.CreateInstance(runnerType, new object[] { this, runnerId, context }) is IProcessTransformationRunner runner)
            {
                if (Runners.TryAdd(runnerId, runner))
                {
                    return runner;
                }
            }
            return default;
        }
    }
}
