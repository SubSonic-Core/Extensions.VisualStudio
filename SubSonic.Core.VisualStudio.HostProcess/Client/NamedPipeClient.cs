using H.Pipes;
using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.HostProcess.Client
{
    public class NamedPipeClient
    {
        private string[] args;

        public NamedPipeClient(string[] args)
        {
            this.args = args;
        }

        public async Task InitializeAsync()
        {
            await using var client = new PipeClient<IProcessTransformationRunFactory>($"ipc://{TransformationRunFactory.TransformationRunFactoryPrefix}/{args[0]}");

            client.MessageReceived += Client_MessageReceived;
            client.Disconnected += Client_Disconnected;
            client.Connected += Client_Connected;
            client.ExceptionOccurred += Client_ExceptionOccurred;

            await client.ConnectAsync().ConfigureAwait(false);

        }

        private void Client_ExceptionOccurred(object sender, H.Pipes.Args.ExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Client_Connected(object sender, H.Pipes.Args.ConnectionEventArgs<IProcessTransformationRunFactory> e)
        {
            throw new NotImplementedException();
        }

        private void Client_Disconnected(object sender, H.Pipes.Args.ConnectionEventArgs<IProcessTransformationRunFactory> e)
        {
            throw new NotImplementedException();
        }

        private void Client_MessageReceived(object sender, H.Pipes.Args.ConnectionMessageEventArgs<IProcessTransformationRunFactory> e)
        {
            throw new NotImplementedException();
        }

        public bool IsRunning { get; protected set; }
    }
}
