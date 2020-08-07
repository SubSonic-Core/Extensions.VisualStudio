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
    [Serializable]
    public class SubSonicTransformationRunFactory
        : TransformationRunFactory
        , IProcessTransformationRunFactory
    {
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
