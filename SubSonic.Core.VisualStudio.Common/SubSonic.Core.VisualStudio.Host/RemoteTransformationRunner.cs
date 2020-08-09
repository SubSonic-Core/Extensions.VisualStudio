using Mono.VisualStudio.TextTemplating.VSHost;
using System;
using System.Reflection;
#if !NET472
using System.Runtime.Loader;
#endif

namespace SubSonic.Core.VisualStudio.Host
{
    [Serializable]
    public class RemoteTransformationRunner
        : TransformationRunner
    {
#if !NET472
        [NonSerialized]
        private readonly RemoteAssemblyLoadContext context;

        public RemoteTransformationRunner(TransformationRunFactory factory, Guid id, RemoteAssemblyLoadContext context) 
            : base(factory, id)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));

            this.context.Resolving += ResolveReferencedAssemblies;
        }

        public override Assembly LoadFromAssemblyName(AssemblyName assemblyName)
        {
            return context.LoadFromAssemblyName(assemblyName);
        }

        protected override void Unload()
        {
            this.context.Resolving -= ResolveReferencedAssemblies;

#if NETCOREAPP
            this.context.Unload();
#endif
        }
#else
        public RemoteTransformationRunner(TransformationRunFactory factory, Guid id)
                : base(factory, id) { }
#endif
    }
}
