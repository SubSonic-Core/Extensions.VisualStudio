using SubSonic.Core.Remoting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Text;

namespace SubSonic.Core.VisualStudio.Common.SubSonic.Core.Remoting.Channels.Ipc
{
    public class IpcServerChannel
        : IServerChannel
    {
        private readonly IDictionary properties;

        public IpcServerChannel(Hashtable properties)
        {
            this.properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public object ChannelData
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
            get;
            protected set;
        }

        public int ChannelPriority
        {
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

        public string PortName
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
            get;
            protected set;
        }

        public Uri GetChannelUri()
        {
            return new Uri($@"ipc:\\{ChannelName}:{PortName}");
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
        public string[] GetUrlsForUri(string objectURI)
        {
            throw new NotImplementedException();
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
        public string Parse(string url, out string objectURI)
        {
            throw new NotImplementedException();
        }

        public IServerChannel SetupChannel()
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
                    else if (key == nameof(PortName))
                    {
                        PortName = Convert.ToString(entry.Value, CultureInfo.InvariantCulture);
                    }
                }
            }

            return this;
        }
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
        public void StartListening(object data)
        {
            throw new NotImplementedException();
        }
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
        public void StopListening(object data)
        {
            throw new NotImplementedException();
        }
    }
}
