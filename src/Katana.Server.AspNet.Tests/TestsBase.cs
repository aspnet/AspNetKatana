using System;
using FakeN.Web;
using Katana.Server.AspNet.Tests.FakeN;

namespace Katana.Server.AspNet.Tests
{
    public class TestsBase
    {
        protected FakeHttpContext NewHttpContext(Uri url)
        {
            return new FakeHttpContext(new FakeHttpRequestEx(url));
        }
    }
}