using H.Pipes;
using Mono.VisualStudio.TextTemplating;
using SubSonic.Core.Remoting.Channels.Ipc;
using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting.Channels
{
    public static class ChannelServices
    {
        [SecuritySafeCritical]
        public static bool RegisterChannel(IpcChannel ipcChannel, bool ensureSecurity = false, CancellationToken cancellationToken = default)
        {
            ipcChannel.Initialize();

            Uri serverUri = ipcChannel.ServerChannel.GetChannelUri();

            return false;
        }

        public static TType Connect<TType>(string uri)
        {
            throw new NotImplementedException();
        }
    }
}
