// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Web;

namespace Microsoft.Owin.Host.SystemWeb.Tests.FakeN
{

    public class FakeHttpResponse : HttpResponseBase
    {
        private readonly NameValueCollection _headers = new();

        public override NameValueCollection Headers => _headers;

        public override bool TrySkipIisCustomErrors { get => false; set { } }

        public override string StatusDescription { get; set; }
    }
}