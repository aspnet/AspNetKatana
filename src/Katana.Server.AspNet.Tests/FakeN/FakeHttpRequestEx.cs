using System;
using FakeN.Web;

namespace Katana.Server.AspNet.Tests.FakeN
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

        public override string PathInfo
        {
            get
            {
                return String.Empty;
            }
        }

        public override bool IsSecureConnection
        {
            get { return false; }
        }
    }
}
