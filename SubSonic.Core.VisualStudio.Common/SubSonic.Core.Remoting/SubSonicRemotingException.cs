using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting
{
    public class SubSonicRemotingException
        : Exception
    {
        public SubSonicRemotingException()
            : base() { }
        public SubSonicRemotingException(string message)
            : base(message) { }

        public SubSonicRemotingException(string message, Exception exception)
            : base(message, exception) { }
    }
}
