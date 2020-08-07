using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using SubSonic.Core.VisualStudio.Host;
using System;
using System.Collections.Generic;


namespace SubSonic.Core.VisualStudio.HostProcess.Server
{
    public class TransformationRunFactoryService
        : ITransformationRunFactoryService
        , IDisposable
    {
        Dictionary<Guid, TransformationRunFactory> RunFactories = new Dictionary<Guid, TransformationRunFactory>();
        private bool disposedValue;

        public bool IsRunning { get; protected set; } = true;

        public IProcessTransformationRunFactory GetTransformationRunFactory(string guidID)
        {
            Guid ID = Guid.Parse(guidID);

            if (RunFactories.ContainsKey(ID))
            {
                return RunFactories[ID];
            }

            SubSonicTransformationRunFactory factory = new SubSonicTransformationRunFactory()
            {
                IsAlive = true
            };

            RunFactories.Add(ID, factory);

            return factory;
        }

        public bool Shutdown()
        {
            return !(IsRunning = false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    RunFactories.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TransformationRunFactoryService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
