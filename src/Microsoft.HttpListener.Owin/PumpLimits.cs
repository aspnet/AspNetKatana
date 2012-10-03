using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.HttpListener.Owin
{
    internal class PumpLimits
    {
        internal PumpLimits(int maxAccepts, int maxRequests)
        {
            MaxOutstandingAccepts = maxAccepts;
            MaxOutstandingRequests = maxRequests;
        }

        internal int MaxOutstandingAccepts { get; private set; }
        internal int MaxOutstandingRequests { get; private set; }
    }
}
