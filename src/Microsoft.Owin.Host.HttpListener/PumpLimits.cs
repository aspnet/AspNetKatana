// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Host.HttpListener
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
