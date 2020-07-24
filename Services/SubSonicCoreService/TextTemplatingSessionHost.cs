using Microsoft.VisualStudio.TextTemplating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Services
{
    public partial class SubSonicCoreService
        : ITextTemplatingSessionHost
    {
        private ITextTemplatingSessionHost SessionHost => templating as ITextTemplatingSessionHost;

        public ITextTemplatingSession Session { get => SessionHost.Session; set => SessionHost.Session = value; }

        public ITextTemplatingSession CreateSession()
        {
            return SessionHost.CreateSession();
        }
    }
}
