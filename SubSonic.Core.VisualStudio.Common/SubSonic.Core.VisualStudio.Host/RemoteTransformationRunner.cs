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
        bool disposed;

#if !NET472
        public RemoteTransformationRunner(TransformationRunFactory factory, Guid id)
            : base(factory, id)
        {
            if (Factory is RemoteTransformationRunFactory remote)
            {
                remote.Context(RunnerId).Resolving += ResolveReferencedAssemblies;
            }
        }

        public override Assembly LoadFromAssemblyName(AssemblyName assemblyName)
        {
            if (Factory is RemoteTransformationRunFactory remote)
            {
                return remote.Context(RunnerId).LoadFromAssemblyName(assemblyName);
            }
            return default;
        }

        protected override void Unload()
        {
            if (Factory is RemoteTransformationRunFactory remote)
            {
                remote.UnloadContext(RunnerId);
            }
        }
#else
        public RemoteTransformationRunner(TransformationRunFactory factory, Guid id)
                : base(factory, id) { }
#endif

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposed)
            {
#if NETSTANDARD || NETCOREAPP
                if (Factory is RemoteTransformationRunFactory remote)
                {
                    remote.Context(RunnerId).Resolving -= ResolveReferencedAssemblies;
                }

                if (disposing)
                {                    
                    Unload();
                }
#endif
                disposed = true;
            }
        }
    }
}
