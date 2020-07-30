using Mono.VisualStudio.TextTemplating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Services
{
    public partial class SubSonicTemplatingService
        : ITextTemplatingSessionHost
    {
        public ITextTemplatingSession Session { get; set; }

        public ITextTemplatingSession CreateSession()
        {
            ITextTemplatingSession session = new TextTemplatingSession();

            session[nameof(TemplateFile)] = Host.TemplateFile;

            return session;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Session.GetObjectData(info, context);
        }
    }
}
