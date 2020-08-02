using System;
using System.Collections.Generic;
using System.Text;
using Mono.TextTemplating;
using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;

namespace SubSonic.Core.VisualStudio.Host
{
    public class SubSonicTransformationRunFactory
        : TransformationRunFactory
        , IProcessTransformationRunFactory
    {
        public static void RegisterChannel(Guid guid)
        {
            throw new NotImplementedException();
        }

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
