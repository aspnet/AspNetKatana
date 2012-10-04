//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

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
