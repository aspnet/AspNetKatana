// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

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

        public override bool IsWebSocketRequest
        {
            get { return true; }
        }

        public override object GetService(Type serviceType)
        {
            return null;
        }
    }
}
