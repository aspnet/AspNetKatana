using System;
using System.Web.Hosting;
using Katana.Engine;
using Katana.Engine.Settings;
using Katana.Engine.Utils;

namespace Katana.Boot.AspNet
{
    public class AspNetStarterAgent : MarshalByRefObject, IRegisteredObject
    {
        AspNetStarterProxy _proxy;

        public IDisposable Start(AspNetStarterProxy proxy, StartParameters parameters)
        {
            _proxy = proxy;
            HostingEnvironment.RegisterObject(this);

            var info = new StartContext
                       {
                           Parameters = parameters,
                           Builder = new AppBuilderWrapper(),
                       };

            var engine = BuildEngine();

            return new Disposable(engine.Start(info).Dispose);
        }

        private static IKatanaEngine BuildEngine()
        {
            var settings = new KatanaSettings();
            TakeDefaultsFromEnvironment(settings);
            return new KatanaEngine(settings);
        }

        private static void TakeDefaultsFromEnvironment(KatanaSettings settings)
        {
            var port = Environment.GetEnvironmentVariable("PORT", EnvironmentVariableTarget.Process);
            int portNumber;
            if (!string.IsNullOrWhiteSpace(port) && int.TryParse(port, out portNumber))
                settings.DefaultPort = portNumber;

            var owinServer = Environment.GetEnvironmentVariable("OWIN_SERVER", EnvironmentVariableTarget.Process);
            if (!string.IsNullOrWhiteSpace(owinServer))
                settings.DefaultServer = owinServer;
        }

        public void RegisterObject(IRegisteredObject obj)
        {
            HostingEnvironment.RegisterObject(obj);
        }

        public void Stop(bool immediate)
        {
            HostingEnvironment.UnregisterObject(this);
            _proxy.Stop(immediate);
        }
    }
}