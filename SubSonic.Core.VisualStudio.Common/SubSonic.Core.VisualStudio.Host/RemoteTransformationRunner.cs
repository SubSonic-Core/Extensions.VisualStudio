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
        public RemoteTransformationRunner(TransformationRunFactory factory, Guid id)
            : base(factory, id)
        {
            RemoteTransformationRunFactory.Context.Resolving += ResolveReferencedAssemblies;
        }

        public override Assembly LoadFromAssemblyName(AssemblyName assemblyName)
        {
            return RemoteTransformationRunFactory.Context.LoadFromAssemblyName(assemblyName);
        }

        protected override void Unload()
        {
            RemoteTransformationRunFactory.Context.Resolving -= ResolveReferencedAssemblies;
#if NETCOREAPP
            RemoteTransformationRunFactory.Context.Unload();
#endif
        }
#else
        public RemoteTransformationRunner(TransformationRunFactory factory, Guid id)
                : base(factory, id) { }
#endif
    }
}
