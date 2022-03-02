// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web;

namespace Microsoft.Owin.Host.SystemWeb.Tests.FakeN
{
    public class FakeHttpContext : HttpContextBase
    {
        private FakeHttpRequestEx _request;
        private FakeHttpResponseEx _response;
        private IDictionary _items = new Dictionary<object, object>();

        public FakeHttpContext(FakeHttpRequestEx request, FakeHttpResponseEx response)
        {
            _request = request;
            _response = response;
        }

        public override HttpRequestBase Request => _request;

        public override HttpResponseBase Response => _response;

        public override IDictionary Items => _items;

        public override IPrincipal User { get; set; }
    }
}