﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using FunctionalTests.Common;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.Security.Cookies
{
    public class CookiesApplyRedirect
    {
        [Theory, Trait("FunctionalTests", "Security")]
        [InlineData(HostType.HttpListener)]
        public void Security_CookiesAuthDefaults(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, CookieApplyRedirectConfiguration);
                var httpClient = new HttpClient();

                // Unauthenticated request
                var response = httpClient.GetAsync(applicationUrl).Result;
                Assert.Equal("custom", response.RequestMessage.RequestUri.ParseQueryString()["custom_redirect_uri"]);
            }
        }

        internal void CookieApplyRedirectConfiguration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                LoginPath = new PathString("/Auth/CookiesLogin"),
                Provider = new CookieAuthenticationProvider()
                {
                    OnApplyRedirect = context =>
                    {
                        context.Response.Redirect(context.RedirectUri + "&custom_redirect_uri=custom");
                    }
                }
            });

            app.UseProtectedResource();
        }
    }
}