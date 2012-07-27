using FakeN.Web;

namespace Katana.Server.AspNet.Tests.FakeN
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