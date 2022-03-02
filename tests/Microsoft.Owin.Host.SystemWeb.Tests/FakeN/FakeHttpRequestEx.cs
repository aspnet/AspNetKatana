// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Microsoft.Owin.Host.SystemWeb.Tests.FakeN
{
    public class FakeHttpRequestEx : FakeHttpRequest
    {
        public FakeHttpRequestEx(Uri url = null, string method = "GET")
            : base(url, method)
        {
        }

        public override string AppRelativeCurrentExecutionFilePath
        {
            get { return Url != null ? "~" + Url.AbsolutePath : "~/"; }
        }

        public override string CurrentExecutionFilePath
        {
            get { return Url != null ? Url.AbsolutePath : "/"; }
        }

        public override string PathInfo
        {
            get { return String.Empty; }
        }

        public override bool IsSecureConnection
        {
            get { return false; }
        }

        public override Stream InputStream
        {
            get { return Stream.Null; }
        }
    }
}
