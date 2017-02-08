// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
