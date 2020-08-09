using Mono.VisualStudio.TextTemplating.VSHost;
using ServiceWire.NamedPipes;
using SubSonic.Core.VisualStudio.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientConsole
{
    class Program
    {
#pragma warning disable IDE0060 // Remove unused parameter
        static void Main(string[] args)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            using(var npClient = new NpClient<ITransformationRunFactoryService>(new NpEndPoint(TransformationRunFactory.TransformationRunFactoryService)))
            {
                foreach(var uri in npClient.Proxy.GetAllChannelUri())
                {
                    Console.Out.WriteLine(uri.ToString());
                }
            }
        }
    }
}
