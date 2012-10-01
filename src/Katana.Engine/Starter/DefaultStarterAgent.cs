using System;
using Katana.Engine.CommandLine;
using Katana.Engine.Settings;
using Katana.Engine.Utils;

namespace Katana.Engine.Starter
{
    public class DefaultStarterAgent : MarshalByRefObject, IKatanaStarter
    {
        public void ResolveAssembliesFromDirectory(string directory)
        {
            DomainManager.ResolveAssembliesFromDirectory(directory);
        }

        public IDisposable Start(StartParameters parameters)
        {
            var info = new StartContext
                       {
                           Parameters = parameters,
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

    }
}