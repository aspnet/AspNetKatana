// -----------------------------------------------------------------------
// <copyright file="FakeHttpContextEx.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using FakeN.Web;

namespace Microsoft.Owin.Host.SystemWeb.Tests.FakeN
{
    public class FakeHttpContextEx : FakeHttpContext
    {
        public FakeHttpContextEx()
            : this(new FakeHttpRequestEx(), new FakeHttpResponseEx())
        {
        }

        public FakeHttpContextEx(FakeHttpRequestEx request, FakeHttpResponseEx response)
            : base(request, response)
        {
        }

        public override bool IsDebuggingEnabled
        {
            get
            {
                return true;
            }
        }

#if !NET40
        public override bool IsWebSocketRequest
        {
            get
            {
                return true;
            }
        }
#endif
    }
}
