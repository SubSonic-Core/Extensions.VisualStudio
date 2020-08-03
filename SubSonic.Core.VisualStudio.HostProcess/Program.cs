using SubSonic.Core.VisualStudio.HostProcess.Client;
using System;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.HostProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new NamedPipeClient(args);

            _ = client.InitializeAsync();

            while (client.IsRunning)
            {
                Task.Delay(10).Wait();
            }
        }
    }
}
