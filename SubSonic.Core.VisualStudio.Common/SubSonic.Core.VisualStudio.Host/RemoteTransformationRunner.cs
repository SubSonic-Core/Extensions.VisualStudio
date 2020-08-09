using Mono.VisualStudio.TextTemplating.VSHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Host
{
    [Serializable]
    public class RemoteTransformationRunner
        : TransformationRunner
    {
        [NonSerialized]
        private readonly RemoteAssemblyLoadContext context;

        public RemoteTransformationRunner(TransformationRunFactory factory, Guid id, RemoteAssemblyLoadContext context) 
            : base(factory, id)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

#if NETSTANDARD || NETCOREAPP || NET5
        protected override AssemblyLoadContext GetLoadContext()
        {
            return context;
        }

        protected override void Unload(AssemblyLoadContext context)
        {
#if !NETSTANDARD
            context.Unload();
#else
            this.context.Unload();
#endif
        }
#endif
        }
}
