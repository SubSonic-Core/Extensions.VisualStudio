using Mono.VisualStudio.TextTemplating.VSHost;
using ServiceWire;
using ServiceWire.NamedPipes;
using SubSonic.Core.VisualStudio.Host;
using System;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace SubSonic.Core.VisualStudio.HostProcess
{
    class Program
    {
#pragma warning disable IDE0060 // Remove unused parameter
        static void Main(string[] args)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            var logger = new Logger(logLevel: LogLevel.Debug);
            var stats = new Stats();

            string pipename = TransformationRunFactory.TransformationRunFactoryService;
            int timeout = 60;

            foreach(string arg in args)
            {
                if (arg.StartsWith(nameof(pipename), StringComparison.OrdinalIgnoreCase))
                {
                    pipename = arg.Split(':')[1];
                }
                else if (arg.StartsWith(nameof(timeout), StringComparison.OrdinalIgnoreCase) &&
                         int.TryParse(arg.Split(':')[1], out int _timeout))
                {
                    timeout = _timeout;
                }
            }

#pragma warning disable IDE0063 // Use simple 'using' statement
            using (TransformationRunFactoryService service = new TransformationRunFactoryService(new Uri($"ipc://{pipename}"), timeout))
#pragma warning restore IDE0063 // Use simple 'using' statement
            using (NpHost host = new NpHost(pipename, logger, stats))
            {
                host.AddService<ITransformationRunFactoryService>(service);

                host.Open();

                Console.Out.WriteLine($"Startup: {DateTime.Now}");

                while(service.IsRunning)
                {
                    Thread.Sleep(10);
                }

                Console.Out.WriteLine($"Shutdown: {DateTime.Now}");
            }
        }
    }
}
