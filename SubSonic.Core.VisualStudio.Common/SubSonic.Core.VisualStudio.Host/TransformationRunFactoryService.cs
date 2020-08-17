using Mono.TextTemplating;
using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using SubSonic.Core.Remoting.Channels.Services;
using SubSonic.Core.VisualStudio.Common;
using System;
using RunFactory = Mono.VisualStudio.TextTemplating.VSHost.TransformationRunFactory;

namespace SubSonic.Core.VisualStudio.Host
{
    [Serializable]
    public class TransformationRunFactoryService
        : BasePipeService
        , ITransformationRunFactoryService
        , IDisposable
    {
        private bool disposedValue;

        [NonSerialized]
        private readonly IProcessTransformationRunFactory runFactory;

        public TransformationRunFactoryService(Uri serviceUri)
            : base(serviceUri)
        {
            runFactory = new RemoteTransformationRunFactory(Guid.NewGuid());
        }

        public Guid GetFactoryId()
        {
            return runFactory.GetFactoryId();
        }

        public bool IsRunFactoryAlive()
        {
            return runFactory.IsRunFactoryAlive();
        }

        public IProcessTransformationRunFactory TransformationRunFactory()
        {
            throw new InvalidOperationException(SubSonicCoreVisualStudioCommonResources.MethodIsStubbedOutForProxyImpersonation);
        }

        public IProcessTransformationRunner CreateTransformationRunner()
        {
            try
            {
                IProcessTransformationRunner runner = runFactory.CreateTransformationRunner();

                Console.Out.WriteLine(SubSonicCoreVisualStudioCommonResources.CreatedTransformationRunner.Format(runner.RunnerId, runner.GetType().FullName));

                return runner;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
            return default;
        }

        public bool DisposeOfRunner(Guid runnerId)
        {
            try
            {
                bool result = runFactory.DisposeOfRunner(runnerId);

                Console.Out.WriteLine(SubSonicCoreVisualStudioCommonResources.DisposedTransformationRunner.Format(runnerId));

                return result;
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
            return default;
        }

        public bool PrepareTransformation(Guid runnerId, ParsedTemplate pt, string content, ITextTemplatingEngineHost host, TemplateSettings settings)
        {
            try
            {
                return runFactory.PrepareTransformation(runnerId, pt, content, host, settings);
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
            return default;
        }

        public ITextTemplatingCallback StartTransformation(Guid runnerId)
        {
            try
            {
                ITextTemplatingCallback result = runFactory.StartTransformation(runnerId);

                if (result.Errors.HasErrors)
                {
                    foreach (var error in result.Errors)
                    {
                        Console.Error.WriteLine($"{error.Location}({error.Location.Line},{error.Location.Column}){((error.ErrorNumber?.Length ?? 0) > 0 ? $": {error.ErrorNumber}" : "")} {(error.IsWarning ? "warning" : "error")}: {error.Message}");
                    }
                }
                else
                {
                    Console.Out.WriteLine(result.TemplateOutput);
                }

                return result;
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
            return default;
        }

        public TemplateErrorCollection GetErrors(Guid runnerId)
        {
            return runFactory.GetErrors(runnerId);
        }

        public bool Shutdown(Guid id)
        {
            foreach (var entry in RunFactory.Runners)
            {
                if (entry.Value is TransformationRunner runner)
                {
                    if (id == runner.Factory.ID)
                    {
                        runFactory.DisposeOfRunner(runner.RunnerId);
                    }
                }
            }

            return IsRunning = RunFactory.Runners.Count > 0;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Shutdown(GetFactoryId());
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
