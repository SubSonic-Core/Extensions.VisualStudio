using Microsoft.VisualStudio.Services.NameResolution;
using Mono.VisualStudio.TextTemplating.VSHost;
using ServiceWire;
using ServiceWire.NamedPipes;
using SubSonic.Core.VisualStudio.HostProcess.Server;
using System.Threading;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.HostProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new Logger(logLevel: LogLevel.Debug);
            var stats = new Stats();

            var pipename = TransformationRunFactory.TransformationRunFactoryService;

            using (TransformationRunFactoryService service = new TransformationRunFactoryService())
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
