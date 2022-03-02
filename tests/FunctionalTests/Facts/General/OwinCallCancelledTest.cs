// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FunctionalTests.Common;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.General
{
    public class OwinCallCancelledTest
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        [InlineData(HostType.IIS)]
        public void OwinCallCancelled(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var serverInstance = new NotificationServer();
                serverInstance.StartNotificationService();

                string applicationUrl = deployer.Deploy(hostType, Configuration);

                try
                {
                    Trace.WriteLine(string.Format("Making a request to url : ", applicationUrl));
                    HttpClient httpClient = new HttpClient();
                    Task<HttpResponseMessage> response = httpClient.GetAsync(applicationUrl);
                    response.Wait(1 * 1000);
                    httpClient.CancelPendingRequests();
                    bool receivedNotification = serverInstance.NotificationReceived.WaitOne(20 * 1000);
                    Assert.True(receivedNotification, "CallCancelled CancellationToken was not issued on cancelling the call");
                }
                finally
                {
                    serverInstance.Dispose();
                }
            }
        }

        internal void Configuration(IAppBuilder app)
        {
            app.Run(context =>
            {
                Stopwatch stopWatch = new Stopwatch();
                Trace.WriteLine("Received client call. Starting stop watch now.");
                stopWatch.Start();

                context.Request.CallCancelled.Register(() =>
                {
                    stopWatch.Stop();
                    Trace.WriteLine(string.Format("Cancellation token triggered. Elapsed time : {0}. Test should succeed", stopWatch.Elapsed));
                    NotificationServer.NotifyClient();
                });

                int retryCount = 0;
                while (retryCount < 3)
                {
                    Thread.CurrentThread.Join(5 * 1000);
                    if (context.Request.CallCancelled.IsCancellationRequested)
                    {
                        break;
                    }
                    retryCount++;
                }

                return context.Response.WriteAsync("FAILURE");
            });
        }
    }
}