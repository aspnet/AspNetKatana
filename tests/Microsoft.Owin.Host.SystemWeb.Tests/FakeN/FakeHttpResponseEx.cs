// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading;

namespace Microsoft.Owin.Host.SystemWeb.Tests.FakeN
{
    public class FakeHttpResponseEx : FakeHttpResponse
    {
        private readonly Stream _outputStream = Stream.Null;
        private int _status;

        public override int StatusCode
        {
            get { return _status; }
            set { _status = value; }
        }

        public override Stream OutputStream
        {
            get { return _outputStream; }
        }

        public override CancellationToken ClientDisconnectedToken
        {
            get { return CancellationToken.None; }
        }
    }
}
