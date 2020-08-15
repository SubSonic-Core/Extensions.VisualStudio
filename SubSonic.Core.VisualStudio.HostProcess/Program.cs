﻿using Mono.VisualStudio.TextTemplating.VSHost;
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

            var pipename = TransformationRunFactory.TransformationRunFactoryService;

#pragma warning disable IDE0063 // Use simple 'using' statement
            using (TransformationRunFactoryService service = new TransformationRunFactoryService(new Uri($"ipc://{TransformationRunFactory.TransformationRunFactoryService}")))
#pragma warning restore IDE0063 // Use simple 'using' statement
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
