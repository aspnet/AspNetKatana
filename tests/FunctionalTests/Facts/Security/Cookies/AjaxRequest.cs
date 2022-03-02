﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using FunctionalTests.Common;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Newtonsoft.Json;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.Security.Cookies
{
    public class FormsAuthenticationFacts
    {
        [Theory, Trait("FunctionalTests", "Security")]
        [InlineData(HostType.IIS)]
        [InlineData(HostType.HttpListener)]
        public void Security_AjaxUnAuthorizedResponses(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, SimulateAjaxRequestConfiguration);

                var httpClient = new HttpClient();
                //Simulate an AJAX request
                httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");

                var response = httpClient.GetAsync(applicationUrl).Result;
                //For AJAX requests the cookie middleware should not send a 302 on a 401 response. 
                //Instead it should send a response back with this below header and a dictionary having the redirect location information
                Assert.NotEmpty(string.Join(",", response.Headers.GetValues("X-Responded-JSON")));
                var ajaxResponseObject = JsonConvert.DeserializeObject<AjaxResponse>(string.Join(",", response.Headers.GetValues("X-Responded-JSON")));
                Assert.Equal<int>(401, ajaxResponseObject.status);
                Assert.Single(ajaxResponseObject.headers);
                Assert.EndsWith("/Auth/CookiesLogin", new Uri(ajaxResponseObject.headers["location"]).AbsolutePath);
                Assert.Equal(new Uri(ajaxResponseObject.headers["location"]).ParseQueryString()["ReturnUrl"], new Uri(applicationUrl).AbsolutePath);
            }
        }

        internal void SimulateAjaxRequestConfiguration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                LoginPath = new PathString("/Auth/CookiesLogin"),
            });

            app.UseProtectedResource();
        }

        public class AjaxResponse
        {
            public int status;

            public Dictionary<string, string> headers;
        }
    }
}