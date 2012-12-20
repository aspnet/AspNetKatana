using System;

namespace Microsoft.Owin.Hosting.Settings
{
    public class DefaultKatanaSettingsProvider : IKatanaSettingsProvider
    {
        public IKatanaSettings GetSettings()
        {
            var settings = new KatanaSettings();
            TakeDefaultsFromEnvironment(settings);
            return settings;
        }

        private static void TakeDefaultsFromEnvironment(KatanaSettings settings)
        {
            string port = Environment.GetEnvironmentVariable("PORT", EnvironmentVariableTarget.Process);
            int portNumber;
            if (!string.IsNullOrWhiteSpace(port) && int.TryParse(port, out portNumber))
            {
                settings.DefaultPort = portNumber;
            }

            string owinServer = Environment.GetEnvironmentVariable("OWIN_SERVER", EnvironmentVariableTarget.Process);
            if (!string.IsNullOrWhiteSpace(owinServer))
            {
                settings.DefaultServer = owinServer;
            }
        }
    }
}