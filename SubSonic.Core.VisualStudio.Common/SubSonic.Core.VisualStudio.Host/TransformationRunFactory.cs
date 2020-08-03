using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mono.TextTemplating;
using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;

#if NETFRAMEWORK
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Remoting.Channels.Ipc;
#endif

namespace SubSonic.Core.VisualStudio.Host
{
    public class SubSonicTransformationRunFactory
        : TransformationRunFactory
        , IProcessTransformationRunFactory
    {
        public static bool RegisterIpcChannel()
        {
            return RegisterIpcChannel(Guid.NewGuid());
        }

#if NETFRAMEWORK
        public static bool RegisterIpcChannel(Guid guid)
        {
            BinaryServerFormatterSinkProvider serverSinkProvider = new BinaryServerFormatterSinkProvider
            {
                TypeFilterLevel = TypeFilterLevel.Full
            };
            IDictionary properties = new Hashtable();
            properties["portName"] = guid.ToString();
            properties["connectionTimeout"] = 0x3e8;
            ChannelServices.RegisterChannel(new IpcChannel(properties, new BinaryClientFormatterSinkProvider(), serverSinkProvider), false);

            return true;    // we got here without throwing an exception
        }
#elif NETSTANDARD
        public static bool RegisterIpcChannel(Guid guid)
        {
            return false;   // not implemented at this time
        }
#endif

        public override IProcessTransformationRun CreateTransformationRun(Type runnerType, ParsedTemplate pt, ResolveEventHandler resolver)
        {
            throw new NotImplementedException();
        }

        public override string RunTransformation(IProcessTransformationRun transformationRun)
        {
            throw new NotImplementedException();
        }

        
    }
}
