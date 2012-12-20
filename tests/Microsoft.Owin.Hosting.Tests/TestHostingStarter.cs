using System;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Starter;
using Microsoft.Owin.Hosting.Tests;

[assembly: HostingStarter(typeof(TestHostingStarter))]

namespace Microsoft.Owin.Hosting.Tests
{
    public class TestHostingStarter : IHostingStarter
    {
        private readonly IKatanaEngine _engine;

        public TestHostingStarter(IKatanaEngine engine)
        {
            _engine = engine;
        }

        public IDisposable Start(StartParameters parameters)
        {
            throw new NotImplementedException();
        }
    }
}