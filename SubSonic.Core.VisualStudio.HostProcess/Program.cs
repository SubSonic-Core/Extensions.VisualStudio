using Mono.VisualStudio.TextTemplating.VSHost;
using ServiceWire;
using ServiceWire.NamedPipes;
using SubSonic.Core.VisualStudio.Host;
using SubSonic.Core.VisualStudio.HostProcess.Server;
using System;
using System.Threading;

namespace SubSonic.Core.VisualStudio.HostProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new Logger(logLevel: LogLevel.Debug);
            var stats = new Stats();

            var pipename = TransformationRunFactory.TransformationRunFactoryService;

            using (RemoteTransformationRunFactoryService service = new RemoteTransformationRunFactoryService(new Uri($"ipc://{TransformationRunFactory.TransformationRunFactoryService}")))
            using (NpHost host = new NpHost(pipename, logger, stats))
            {
                host.AddService<ITransformationRunFactoryService>(service);

                host.Open();

                while(service.IsRunning)
                {
                    Thread.Sleep(10);
                }
            }
        }
    }
}
