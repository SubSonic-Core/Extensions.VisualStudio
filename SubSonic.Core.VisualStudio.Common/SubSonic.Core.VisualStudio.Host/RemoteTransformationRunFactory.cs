using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if !NETFRAMEWORK
using System.Runtime.Loader;
#endif
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Host
{
    [Serializable]
    public class RemoteTransformationRunFactory
         : TransformationRunFactory
    {
#if NETSTANDARD || NETCOREAPP
        public static RemoteAssemblyLoadContext Context { get; } = new RemoteAssemblyLoadContext();
#endif
        public RemoteTransformationRunFactory(Guid id)
            : base(id) { }

        public override IProcessTransformationRunner CreateTransformationRunner()
        {
            Guid runnerId = Guid.NewGuid();

#if NETSTANDARD || NETCOREAPP
            IProcessTransformationRunner runner = new RemoteTransformationRunner(this, runnerId);
#else
            IProcessTransformationRunner runner = new RemoteTransformationRunner(this, runnerId);
#endif

            if (Runners.TryAdd(runnerId, runner))
            {
                return runner;
            }

            return default;
        }
    }
}
