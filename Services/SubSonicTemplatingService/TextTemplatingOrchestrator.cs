using Microsoft.VisualStudio.TextTemplating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Services
{
    public partial class SubSonicTemplatingService
        : ITextTemplatingOrchestrator
    {
        private ITextTemplatingOrchestrator Orchestrator => this.templating as ITextTemplatingOrchestrator;

        public event EventHandler<TransformingAllTemplatesEventArgs> TransformingAllTemplates
        {
            add
            {
                Orchestrator.TransformingAllTemplates += value;
            }
            remove
            {
                Orchestrator.TransformingAllTemplates -= value;
            }
        }
        public event EventHandler<TransformedAllTemplatesEventArgs> TransformedAllTemplates
        {
            add
            {
                Orchestrator.TransformedAllTemplates += value;
            }
            remove
            {
                Orchestrator.TransformedAllTemplates -= value;
            }
        }
    }
}
