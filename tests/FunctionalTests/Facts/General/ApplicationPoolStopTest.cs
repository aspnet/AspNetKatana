// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Threading;
using FunctionalTests.Common;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.General
{
    public class ApplicationPoolStopTest
    {
        private CancellationToken appDisposingTokenOnHostProperties;

        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        [InlineData(HostType.IIS)]
        public void ApplicationPoolStop(HostType hostType)
        {
            var serverInstance = new NotificationServer();
            serverInstance.StartNotificationService();
            try
            {
                using (ApplicationDeployer deployer = new ApplicationDeployer())
                {
                    string applicationUrl = deployer.Deploy(hostType, Configuration);
                    Assert.True(HttpClientUtility.GetResponseTextFromUrl(applicationUrl) == "SUCCESS");

                    if (hostType == HostType.IIS)
                    {
                        string webConfig = deployer.GetWebConfigPath();
                        string webConfigContent = File.ReadAllText(webConfig);
                        File.WriteAllText(webConfig, webConfigContent);
                    }
                }

                bool receivedNotification = serverInstance.NotificationReceived.WaitOne(20 * 1000);
                Assert.True(receivedNotification, "Cancellation token was not issued on closing host");
            }
            finally
            {
                serverInstance.Dispose();
            }
        }

        public void Configuration(IAppBuilder app)
        {
            appDisposingTokenOnHostProperties = app.Properties.Get<CancellationToken>("host.OnAppDisposing");
            appDisposingTokenOnHostProperties.Register(() =>
            {
                NotificationServer.NotifyClient();
            });

            app.Run(context =>
            {
                var perRequestHostDisposingToken = context.Get<CancellationToken>("host.OnAppDisposing");
                if (perRequestHostDisposingToken.GetHashCode() != this.appDisposingTokenOnHostProperties.GetHashCode())
                {
                    return context.Response.WriteAsync("FAILURE");
                }

                return context.Response.WriteAsync("SUCCESS");
            });
        }
    }
}