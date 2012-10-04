//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.IO;
using FakeN.Web;

namespace Microsoft.AspNet.Owin.Tests.FakeN
{
    public class FakeHttpResponseEx : FakeHttpResponse
    {
        private int _status;
        private Stream _outputStream = Stream.Null;

        public override int StatusCode
        {
            get { return _status; }
            set { _status = value; }
        }

        public override Stream OutputStream
        {
            get { return _outputStream; }
        }
    }
}
