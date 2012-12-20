using System;
using Microsoft.Owin.Hosting.Services;

namespace Microsoft.Owin.Hosting.Starter
{
    public class DirectHostingStarter : IHostingStarter
    {
        private readonly IKatanaEngine _engine;

        public DirectHostingStarter(IKatanaEngine engine)
        {
            _engine = engine;
        }

        public IHostingStarter CreateIntance(IServiceProvider services)
        {
            return new DirectHostingStarter(
                services.GetService<IKatanaEngine>());
        }

        public IDisposable Start(StartParameters parameters)
        {
            return _engine.Start(new StartContext { Parameters = parameters });
        }
    }
}
