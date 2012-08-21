using FakeN.Web;

namespace Microsoft.AspNet.Owin.Tests.FakeN
{
    public class FakeHttpResponseEx : FakeHttpResponse
    {
        private int _status;
        public override int StatusCode
        {
            get { return _status; }
            set { _status = value; }
        }
    }
}