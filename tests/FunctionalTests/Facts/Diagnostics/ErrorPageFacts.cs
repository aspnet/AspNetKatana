// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using FunctionalTests.Common;
using Microsoft.Owin.Diagnostics;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.Diagnostics
{
    public class ErrorPageFacts
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.IIS)]
        [InlineData(HostType.HttpListener)]
        public void ErrorPage(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, ErrorPageConfiguration);

                HttpResponseMessage response;
                var responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl, out response).ToLower();

                Assert.Contains("argumentexception", responseText);
                Assert.Contains("environment", responseText);
                Assert.Contains("stack", responseText);
                Assert.Contains("query", responseText);
                Assert.Contains("cookies", responseText);
                Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType.ToLower());
            }
        }

        internal void ErrorPageConfiguration(IAppBuilder app)
        {
            app.UseErrorPage(new ErrorPageOptions()
            {
                ShowCookies = true,
                ShowEnvironment = true,
                ShowExceptionDetails = true,
                ShowHeaders = true,
                ShowQuery = true,
                ShowSourceCode = true,
                SourceCodeLineCount = 10
            });
            app.Run(context =>
                {
                    if (context != null)
                    {
                        throw new ArgumentException("environment");
                    }
                    return context.Response.WriteAsync("Test failed..");
                });
        }
    }
}