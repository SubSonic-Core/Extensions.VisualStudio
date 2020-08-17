using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using System;
using System.Collections.Concurrent;
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
        private static readonly ConcurrentDictionary<Guid, AssemblyLoadContext> LoadContexts = new ConcurrentDictionary<Guid, AssemblyLoadContext>();
#endif
        public RemoteTransformationRunFactory(Guid id)
            : base(id) { }

        public override IProcessTransformationRunner CreateTransformationRunner()
        {
            Guid runnerId = Guid.NewGuid();

#if NETSTANDARD || NETCOREAPP
            if (LoadContexts.TryAdd(runnerId, new RemoteAssemblyLoadContext()))
            {
                IProcessTransformationRunner runner = new RemoteTransformationRunner(this, runnerId);

                if (Runners.TryAdd(runnerId, runner))
                {
                    return runner;
                }
            }
#else
            IProcessTransformationRunner runner = new RemoteTransformationRunner(this, runnerId);

            if (Runners.TryAdd(runnerId, runner))
            {
                return runner;
            }
#endif

            return default;
        }

#if NETSTANDARD || NETCOREAPP
        public AssemblyLoadContext Context(Guid runnerId)
        {
            if (LoadContexts.TryGetValue(runnerId, out AssemblyLoadContext context))
            {
                return context;
            }
            return default;
        }

        public void UnloadContext(Guid runnerId)
        {
            if (LoadContexts.TryRemove(runnerId, out AssemblyLoadContext context))
            {
#if NETCOREAPP
                context.Unload();
#endif
            }
        }
#endif

    }
}
