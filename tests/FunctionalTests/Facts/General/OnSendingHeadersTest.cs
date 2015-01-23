// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using FunctionalTests.Common;
using Microsoft.Owin;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.General
{
    public class OnSendingHeadersTest
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        [InlineData(HostType.IIS)]
        public void OnSendingHeaders(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy<OnSendingHeadersTest>(hostType);
                HttpResponseMessage httpResponseMessage = null;
                HttpClientUtility.GetResponseTextFromUrl(applicationUrl, out httpResponseMessage);
                string receivedHeaderValue = httpResponseMessage.Headers.GetValues("DummyHeader").First();
                Assert.True(receivedHeaderValue == "DummyHeaderValue,DummyHeaderValue",
                    string.Format("Expected header values : {0}. Received header values : {1}", "DummyHeaderValue,DummyHeaderValue", receivedHeaderValue));
            }
        }

        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.Use((context, next) =>
            {
                context.Response.OnSendingHeaders(InsertHeaderOnSendingBack, context);
                return next();
            });

            appBuilder.Use((context, next) =>
            {
                context.Response.OnSendingHeaders(InsertHeaderOnSendingBack, context);
                return next();
            });

            appBuilder.Run(context =>
                {
                    return context.Response.WriteAsync("SUCCESS");
                });
        }

        private void InsertHeaderOnSendingBack(object state)
        {
            var owinContext = (IOwinContext)state;
            if (owinContext.Response.Headers.ContainsKey("DummyHeader"))
            {
                owinContext.Response.Headers.Append("DummyHeader", "DummyHeaderValue");
            }
            else
            {
                owinContext.Response.Headers.Add("DummyHeader", new string[] { "DummyHeaderValue" });
            }
        }
    }
}