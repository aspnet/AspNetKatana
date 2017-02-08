// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using Microsoft.Owin.Hosting;
using Owin;

namespace FunctionalTests.Common
{
    internal class NotificationServer : IDisposable
    {
        public AutoResetEvent NotificationReceived;
        private const string NotificationServerUri = "http://localhost:5999";
        private IDisposable server;

        public NotificationServer()
        {
            NotificationReceived = new AutoResetEvent(false);
        }

        public static void NotifyClient()
        {
            var httpClient = new HttpClient();
            httpClient.GetAsync(NotificationServerUri);
        }

        public void StartNotificationService()
        {
            server = WebApp.Start(NotificationServerUri, builder =>
                {
                    builder.Run(context =>
                        {
                            NotificationReceived.Set();
                            return context.Response.WriteAsync("Hello world");
                        });
                });
        }

        public void Dispose()
        {
            if (server != null)
            {
                try
                {
                    server.Dispose();
                }
                catch (Exception)
                {
                    //Ignore exceptions
                }
            }
        }
    }
}