// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Threading;
using FakeN.Web;

#if !NET40
#endif

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

#if !NET40
        public override CancellationToken ClientDisconnectedToken
        {
            get { return CancellationToken.None; }
        }
#endif
    }
}
