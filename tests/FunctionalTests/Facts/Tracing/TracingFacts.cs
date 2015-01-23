// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FunctionalTests.Common;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.Tracing
{
    public class TracingFacts
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        [InlineData(HostType.IIS)]
        public void CreateLogger(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var service = new NotificationServer();
                service.StartNotificationService();

                string applicationUrl = deployer.Deploy(hostType, CreateLoggerConfiguration);

                Assert.Equal("SUCCESS", HttpClientUtility.GetResponseTextFromUrl(applicationUrl));
                Assert.True(service.NotificationReceived.WaitOne(1 * 2000), "Did not receive all expected traces within expected time");
                service.Dispose();
            }
        }

        public void CreateLoggerConfiguration(IAppBuilder app)
        {
            app.SetLoggerFactory(new LoggerFactory());

            app.Use<LoggingMiddleware1>(app);
            app.Use<LoggingMiddleware2>(app);
        }

        public class LoggingMiddleware1 : OwinMiddleware
        {
            IAppBuilder app;
            public LoggingMiddleware1(OwinMiddleware next, IAppBuilder app)
                : base(next)
            {
                this.app = app;
            }

            public override Task Invoke(IOwinContext context)
            {
                ILogger logger = app.CreateLogger<TracingFacts>();

                logger.WriteInformation("Mw1:Information");
                logger.WriteCritical("Mw1:Critical");
                logger.WriteVerbose("Mw1:Verbose");
                logger.WriteWarning("Mw1:Warning");
                logger.WriteError("Mw1:Error");

                return Next.Invoke(context);
            }
        }

        public class LoggingMiddleware2 : OwinMiddleware
        {
            IAppBuilder app;
            public LoggingMiddleware2(OwinMiddleware next, IAppBuilder app)
                : base(next)
            {
                this.app = app;
            }

            public override Task Invoke(IOwinContext context)
            {
                ILogger logger = app.CreateLogger("LoggingMiddleware2");

                logger.WriteInformation("Mw2:Information");
                logger.WriteCritical("Mw2:Critical");
                logger.WriteVerbose("Mw2:Verbose");
                logger.WriteWarning("Mw2:Warning");
                logger.WriteError("Mw2:Error");

                return context.Response.WriteAsync("SUCCESS");
            }
        }
    }

    public class LoggerFactory : ILoggerFactory
    {
        public ILogger Create(string name)
        {
            return new Logger();
        }
    }

    public class Logger : ILogger
    {
        static List<string> expectedMessages = new List<string>(new string[] { 
            "Mw1:Information", "Mw1:Critical", "Mw1:Verbose", "Mw1:Warning", "Mw1:Error", 
                            "Mw2:Information", "Mw2:Critical", "Mw2:Verbose", "Mw2:Warning", "Mw2:Error"
                         });

        public bool WriteCore(TraceEventType eventType, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            if (expectedMessages.Contains(state.ToString()) &&
                state.ToString().Contains(eventType.ToString()))
            {
                expectedMessages.Remove(state.ToString());
            }

            if (expectedMessages.Count == 0)
            {
                NotificationServer.NotifyClient();
            }

            return true;
        }
    }
}
