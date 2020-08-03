using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Permissions;
using System.Text;

namespace SubSonic.Core.Remoting.Channels.Ipc
{
    public class IpcClientChannel
        : IClientChannel
    {
        private readonly IDictionary properties;
        //protected IClientChannelSinkProvider provider;

        public IpcClientChannel()
        {
            ChannelPriority = 1;
            ChannelName = "ipc client";
        }

        public IpcClientChannel(IDictionary properties)
            : this()
        {
            this.properties = properties ?? throw new ArgumentNullException(nameof(properties));
            //this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public int ChannelPriority {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
            get;
            protected set;
        }

        public string ChannelName
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
            get;
            protected set;
        }

        public bool IsSecured
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
            get;
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
            set;
        }

        public IClientChannel SetupChannel()
        {
            if (properties != null)
            {
                foreach (DictionaryEntry entry in properties)
                {
                    if (entry.Key is string key)
                    {
                        if (key == nameof(ChannelName))
                        {
                            ChannelName = (string)entry.Value;
                        }
                        else if (key == nameof(ChannelPriority))
                        {
                            ChannelPriority = Convert.ToInt32(entry.Value, CultureInfo.InvariantCulture);
                        }
                        else if (key == nameof(IsSecured))
                        {
                            IsSecured = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                        }
                    }
                }
            }

            return this;
        }

        //protected IClientChannelSinkProvider CreateDefaultClientProviderChain()
        //{
        //    throw new NotImplementedException();
        //}

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
        public IMessageSink CreateMessageSink(string url, object remoteChannelData, out string objectURI)
        {
            throw new NotImplementedException();
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
        public string Parse(string url, out string objectURI)
        {
            throw new NotImplementedException();
        }
    }
}
