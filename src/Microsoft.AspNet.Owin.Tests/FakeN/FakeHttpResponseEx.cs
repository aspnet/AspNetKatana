using System.IO;
using FakeN.Web;

namespace Microsoft.AspNet.Owin.Tests.FakeN
{
    public class FakeHttpResponseEx : FakeHttpResponse
    {
        private int _status;
        Stream _outputStream;

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
