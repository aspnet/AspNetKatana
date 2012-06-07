using FakeN.Web;

namespace Katana.Server.AspNet.Tests.FakeN
{
    public class FakeHttpResponseEx : FakeHttpResponse
    {
        private string _status;
        public override string Status
        {
            get { return _status; }
            set { _status = value; }
        }
    }
}