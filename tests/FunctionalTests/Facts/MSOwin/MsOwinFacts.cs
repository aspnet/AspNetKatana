// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Headers;
using FunctionalTests.Common;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.MSOwin
{
    using System;
    using kvp = System.Collections.Generic.KeyValuePair<string, string>;

    public class MsOwinFacts
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.IIS)]
        [InlineData(HostType.HttpListener)]
        public void UseHandlerAndReadFormParametersAndQuery(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, ReadFormParametersConfiguration) + "?QUERY%name1=QueryValue1&Query3=~!@$ % ^*()_+1Aa&QUEry2=QUERYVALUE2";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml")); ;
                var response = client.PostAsync(applicationUrl, new FormUrlEncodedContent(new kvp[] { new kvp("input1", "~!@#$%^&*()_+1Aa"), new kvp("input2", "FormInput2") })).Result;
                Assert.Equal("ReadFormParameters", response.Content.ReadAsStringAsync().Result);
            }
        }

        internal void ReadFormParametersConfiguration(IAppBuilder app)
        {
            app.Use((context, next) =>
            {
                context.Set<bool>("HandlerInvoked1", true);
                return next();
            });

            app.Use((context, next) =>
            {
                context.Set<bool>("HandlerInvoked2", true);
                return next();
            });

            app.Use((context, next) =>
                {
                    if (context.Request.Query["QUERY%name1"] != "QueryValue1" ||
                        context.Request.Query["Query2"] != "QUERYVALUE2" ||
                        context.Request.Query["Query3"] != "~!@$ % ^*()_ 1Aa")
                    {
                        context.Response.WriteAsync(string.Empty);
                    }

                    return next();
                });

            app.Run((context) =>
            {
                //Trying to read ReadFormAsync() twice - to make sure we return values every time not just the first time. 
                //Bug# https://github.com/Katana/katana/issues/577
                var form = context.Request.ReadFormAsync().Result;
                if (form.Get("INPut1") != "~!@#$%^&*()_+1Aa" || form.Get("input2") != "FormInput2")
                {
                    throw new Exception("Cannot find form parameters");
                }

                context.Request.ReadFormAsync().ContinueWith(result =>
                {
                    var formData = result.Result;

                    if (context.Request.Accept == "text/xml" && formData.Get("INPut1") == "~!@#$%^&*()_+1Aa" && formData.Get("input2") == "FormInput2" &&
                        context.Get<bool>("HandlerInvoked1") && context.Get<bool>("HandlerInvoked2"))
                    {
                        context.Response.WriteAsync("ReadFormParameters");
                    }
                }).Wait(1 * 1000);

                return context.Response.WriteAsync(string.Empty);
            });
        }
    }
}