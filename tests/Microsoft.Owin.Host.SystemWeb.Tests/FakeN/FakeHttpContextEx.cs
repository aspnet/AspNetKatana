// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
            get { return true; }
        }

#if !NET40
        public override bool IsWebSocketRequest
        {
            get { return true; }
        }
#endif
    }
}
