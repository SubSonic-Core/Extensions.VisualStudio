using SubSonic.Core.VisualStudio.Common.SubSonic.Core.Remoting.Channels.Ipc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Text;

namespace SubSonic.Core.Remoting.Channels.Ipc
{
    public class IpcChannel
        : IChannelReceiver, IChannel, IChannelSender, ISecurableChannel
    {
        private readonly IDictionary properties;
        public IClientChannel ClientChannel;
        public IServerChannel ServerChannel;

        public IpcChannel(IDictionary properties)
        {
            this.properties = properties ?? throw new ArgumentNullException(nameof(properties));

            ChannelPriority = 20;
            ChannelName = "ipc";
        }

        public object ChannelData
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
            get
            {
                return ServerChannel?.ChannelData ?? default;
            }
        }

        public int ChannelPriority {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
            get; 
            protected set; 
        }

        public string ChannelName {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
            get; 
            protected set; 
        }

        public bool IsSecured
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
            get
            {
                return ClientChannel?.IsSecured ?? ServerChannel?.IsSecured ?? default;
            }
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
            set
            {
                if (ClientChannel != null)
                {
                    ClientChannel.IsSecured = value;
                }
                if (ServerChannel != null)
                {
                    ServerChannel.IsSecured = value;
                }
            }
        }

        public IpcChannel Initialize()
        {
            Hashtable
                clientProperties = new Hashtable(),
                serverProperties = new Hashtable();

            bool server = false;

            if (ClientChannel != null)
            {   // will never initialize twice
                return this;
            }

            foreach (DictionaryEntry entry in properties)
            {
                if (entry.Key is string key)
                {
                    if (key == nameof(ChannelName))
                    {
                        ChannelName = (string)entry.Value;
                    }
                    if (key == nameof(ChannelPriority))
                    {
                        ChannelPriority = Convert.ToInt32(entry.Value, CultureInfo.InvariantCulture);
                    }
                    if (key == nameof(IServerChannel.PortName))
                    {
                        serverProperties[entry.Key] = entry.Value;
                        continue;
                    }
                    else
                    {
                        serverProperties[entry.Key] = entry.Value;
                        clientProperties[entry.Key] = entry.Value;
                        continue;
                    }
                }
            }

            server = serverProperties.ContainsKey(nameof(IServerChannel.PortName));

            ClientChannel = new IpcClientChannel(clientProperties).SetupChannel();
            if (server)
            {
                ServerChannel = new IpcServerChannel(serverProperties).SetupChannel();
            }

            return this;
        }

        public IMessageSink CreateMessageSink(string url, object remoteChannelData, out string objectURI)
        {
            throw new NotImplementedException();
        }

        public string[] GetUrlsForUri(string objectURI)
        {
            throw new NotImplementedException();
        }

        public string Parse(string url, out string objectURI)
        {
            throw new NotImplementedException();
        }

        public void StartListening(object data)
        {
            throw new NotImplementedException();
        }

        public void StopListening(object data)
        {
            throw new NotImplementedException();
        }
    }
}
