// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Web;

namespace Microsoft.Owin.Host.SystemWeb.Tests.FakeN
{
    public class FakeHttpRequest : HttpRequestBase
    {
        private Uri _url;
        private string _method;
        private NameValueCollection _headers = new();
        private NameValueCollection _serverVars = new();

        public FakeHttpRequest()
        {
        }

        public FakeHttpRequest(Uri url, string method)
        {
            _url = url;
            _method = method;
        }

        public override Uri Url => _url;

        public override string HttpMethod => _method;

        public override NameValueCollection Headers => _headers;

        public override NameValueCollection ServerVariables => _serverVars;

        public override bool IsLocal => true;
    }
}