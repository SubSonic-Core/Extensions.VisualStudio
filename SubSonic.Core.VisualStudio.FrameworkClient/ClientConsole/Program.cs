using Mono.VisualStudio.TextTemplating.VSHost;
using ServiceWire.NamedPipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var npClient = new NpClient<ITransformationRunFactoryService>(new NpEndPoint(TransformationRunFactory.TransformationRunFactoryService)))
            {
                var runFactory = npClient.Proxy.GetTransformationRunFactory(Guid.NewGuid().ToString());
            }
        }
    }
}
