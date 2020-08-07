using Mono.VisualStudio.TextTemplating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Host
{
    public interface ITransformationRunFactoryService
    {
        /// <summary>
		/// Starts up a transformation run factory
		/// </summary>
		/// <returns>rpc reference to a transformation run factory</returns>
		IProcessTransformationRunFactory TransformationRunFactory(Guid id);
    }
}
