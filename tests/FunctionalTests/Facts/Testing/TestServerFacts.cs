// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Owin;
using Xunit;
using kvp = System.Collections.Generic.KeyValuePair<string, string>;

namespace FunctionalTests.Facts.Testing
{
    public class TestServerFacts
    {
        [Fact]
        public void TestServer_Create()
        {
            var expectedResponseText = System.Guid.NewGuid().ToString();
            Action<IAppBuilder> configuration = (app) =>
            {
                app.Run(context =>
                {
                    return context.Response.WriteAsync(expectedResponseText);
                });
            };

            using (var server = TestServer.Create(configuration))
            {
                var response = server.CreateRequest("/").SendAsync("GET").Result.Content.ReadAsStringAsync().Result;
                Assert.Equal<string>(expectedResponseText, response);
            }
        }

        [Fact]
        public void TestServer_ConfigurationThroughConstructor()
        {
            using (var server = TestServer.Create<TestServerFacts>())
            {
                var response = server.CreateRequest("/?QUERY%name1=QueryValue1&Query3=~!@$ % ^*()_+1Aa&QUEry2=QUERYVALUE2")
                    .And(request => request.Content = new FormUrlEncodedContent(new kvp[] { new kvp("input1", "~!@#$%^&*()_+1Aa"), new kvp("input2", "FormInput2") }))
                    .AddHeader("Custom01", "Custom01Value")
                    .SendAsync("POST").Result;

                Assert.Equal<HttpStatusCode>(HttpStatusCode.Unauthorized, response.StatusCode);
                Assert.Equal<string>("TestServer_ConfigurationThroughConstructor_Result", response.Content.ReadAsStringAsync().Result);
            }
        }

        public void Configuration(IAppBuilder app)
        {
            app.Run(context =>
                    {
                        if (context.Request.Query["QUERY%name1"] != "QueryValue1" ||
                        context.Request.Query["Query2"] != "QUERYVALUE2" ||
                        context.Request.Query["Query3"] != "~!@$ % ^*()_ 1Aa")
                        {
                            return context.Response.WriteAsync(string.Empty);
                        }

                        var form = context.Request.ReadFormAsync().Result;
                        if (form.Get("INPut1") != "~!@#$%^&*()_+1Aa" || form.Get("input2") != "FormInput2")
                        {
                            throw new Exception("Cannot find form parameters");
                        }

                        context.Response.StatusCode = 401;
                        return context.Response.WriteAsync("TestServer_ConfigurationThroughConstructor_Result");
                    });
        }

        [Fact]
        public void TestServer_VerifyDictionaryKeys()
        {
            var expectedResponseText = System.Guid.NewGuid().ToString();
            using (var server = TestServer.Create(app =>
                {
                    app.Run(context =>
                        {
                            if (context.Request.Headers["HEader1"] != "headervalue1")
                            {
                                throw new Exception("Missing header header1");
                            }

                            if (context.Request.Headers["host"] != "localhost")
                            {
                                throw new Exception("Missing header header1");
                            }

                            ThrowIfKeyNotFound<Stream>(context, "owin.RequestBody");
                            ThrowIfKeyNotFound<Stream>(context, "owin.ResponseBody");
                            ThrowIfKeyNotFound<IDictionary<string, string[]>>(context, "owin.RequestHeaders");
                            ThrowIfKeyNotFound<IDictionary<string, string[]>>(context, "owin.ResponseHeaders");
                            ThrowIfKeyNotFound<string>(context, "owin.RequestMethod");
                            ThrowIfKeyNotFound<string>(context, "owin.RequestPath");
                            ThrowIfKeyNotFound<string>(context, "owin.RequestPathBase");
                            ThrowIfKeyNotFound<string>(context, "owin.RequestProtocol");
                            ThrowIfKeyNotFound<string>(context, "owin.RequestQueryString");
                            ThrowIfKeyNotFound<string>(context, "owin.RequestScheme");
                            ThrowIfKeyNotFound<TextWriter>(context, "host.TraceOutput");
                            ThrowIfKeyNotFound<CancellationToken>(context, "owin.CallCancelled");
                            ThrowIfKeyNotFound<string>(context, "owin.Version");
                            ThrowIfKeyNotFound<string>(context, "host.AppName");
                            ThrowIfKeyNotFound<Action<Action<object>, object>>(context, "server.OnSendingHeaders");
                            ThrowIfKeyNotFound<bool>(context, "server.IsLocal");

                            return context.Response.WriteAsync(expectedResponseText);
                        });
                }))
            {
                var response = server.CreateRequest("/").AddHeader("header1", "headervalue1").SendAsync("GET").Result.Content.ReadAsStringAsync().Result;
                Assert.Equal<string>(expectedResponseText, response);
            }
        }

        private void ThrowIfKeyNotFound<T>(IOwinContext context, string key)
        {
            if (context.Get<T>(key) == null)
            {
                throw new Exception(string.Format("Key with name '{0}' cannot be found with type '{1}", key, typeof(T).Name));
            }
        }
    }
}