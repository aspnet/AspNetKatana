// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Owin.Testing.Tests
{
    public class OwinClientHandlerTests
    {
        [Fact]
        public void LeadingQuestionMarkInQueryIsRemoved()
        {
            /* http://katanaproject.codeplex.com/workitem/22
             * 
             * Summary
             * 
             * The owin spec for the "owin.RequestQueryString" key: 
             *    
             *    A string containing the query string component of the HTTP request URI,
             *    without the leading “?” (e.g., "foo=bar&baz=quux"). The value may be an
             *    empty string.
             *    
             *  request.RequestUri.Query does not remove the leading '?'. This causes
             *  problems with hosts that then subsequently join the path and querystring
             *  resulting in a '??' (such as signalr converting the env dict to a ServerRequest) 
             */

            IDictionary<string, object> env = null;
            var handler = new OwinClientHandler(dict =>
            {
                env = dict;
                return Task.FromResult(0);
            });
            var httpClient = new HttpClient(handler);
            string query = "a=b";
            httpClient.GetAsync("http://example.com?" + query).Wait();
            Assert.Equal(query, env["owin.RequestQueryString"]);
        }

        [Fact]
        public void ExpectedKeysAreAvailable()
        {
            var handler = new OwinClientHandler(env =>
            {
                IOwinContext context = new OwinContext(env);

                Assert.Equal("1.0", context.Get<string>("owin.Version"));
                Assert.NotNull(context.Get<CancellationToken>("owin.CallCancelled"));
                Assert.Equal("HTTP/1.1", context.Request.Protocol);
                Assert.Equal("GET", context.Request.Method);
                Assert.Equal("https", context.Request.Scheme);
                Assert.Equal(string.Empty, context.Get<string>("owin.RequestPathBase"));
                Assert.Equal("/A/Path/and/file.txt", context.Get<string>("owin.RequestPath"));
                Assert.Equal("and=query", context.Get<string>("owin.RequestQueryString"));
                Assert.NotNull(context.Request.Body);
                Assert.NotNull(context.Get<IDictionary<string, string[]>>("owin.RequestHeaders"));
                Assert.NotNull(context.Get<IDictionary<string, string[]>>("owin.ResponseHeaders"));
                Assert.NotNull(context.Response.Body);
                Assert.Equal(200, context.Get<int>("owin.ResponseStatusCode"));
                Assert.Null(context.Get<string>("owin.ResponseReasonPhrase"));

                Assert.Equal("example.com", context.Request.Headers.Get("Host"));

                return Task.FromResult(0);
            });
            var httpClient = new HttpClient(handler);
            httpClient.GetAsync("https://example.com/A/Path/and/file.txt?and=query").Wait();
        }

        [Fact]
        public async Task ResubmitRequestWorks()
        {
            int requestCount = 1;
            var handler = new OwinClientHandler(env =>
            {
                IOwinContext context = new OwinContext(env);
                int read = context.Request.Body.Read(new byte[100], 0, 100);
                Assert.Equal(11, read);

                context.Response.Headers["TestHeader"] = "TestValue:" + requestCount++;
                return Task.FromResult(0);
            });

            HttpMessageInvoker invoker = new HttpMessageInvoker(handler);
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, "https://example.com/");
            message.Content = new StringContent("Hello World");

            HttpResponseMessage response = await invoker.SendAsync(message, CancellationToken.None);
            Assert.Equal("TestValue:1", response.Headers.GetValues("TestHeader").First());

            response = await invoker.SendAsync(message, CancellationToken.None);
            Assert.Equal("TestValue:2", response.Headers.GetValues("TestHeader").First());
        }

        [Fact]
        public async Task MiddlewareOnlySetsHeaders()
        {
            var handler = new OwinClientHandler(env =>
            {
                IOwinContext context = new OwinContext(env);

                context.Response.Headers["TestHeader"] = "TestValue";
                return Task.FromResult(0);
            });
            var httpClient = new HttpClient(handler);
            HttpResponseMessage response = await httpClient.GetAsync("https://example.com/");
            Assert.Equal("TestValue", response.Headers.GetValues("TestHeader").First());
        }

        [Fact]
        public async Task BlockingMiddlewareShouldNotBlockClient()
        {
            ManualResetEvent block = new ManualResetEvent(false);
            var handler = new OwinClientHandler(env =>
            {
                block.WaitOne();
                return Task.FromResult(0);
            });
            var httpClient = new HttpClient(handler);
            Task<HttpResponseMessage> task = httpClient.GetAsync("https://example.com/");
            Assert.False(task.IsCompleted);
            Assert.False(task.Wait(50));
            block.Set();
            HttpResponseMessage response = await task;
        }

        [Fact]
        public async Task HeadersAvailableBeforeBodyFinished()
        {
            ManualResetEvent block = new ManualResetEvent(false);
            var handler = new OwinClientHandler(env =>
            {
                IOwinContext context = new OwinContext(env);
                context.Response.Headers["TestHeader"] = "TestValue";
                context.Response.Write("BodyStarted,");
                block.WaitOne();
                context.Response.Write("BodyFinished");
                return Task.FromResult(0);
            });
            var httpClient = new HttpClient(handler);
            HttpResponseMessage response = await httpClient.GetAsync("https://example.com/",
                HttpCompletionOption.ResponseHeadersRead);
            Assert.Equal("TestValue", response.Headers.GetValues("TestHeader").First());
            block.Set();
            Assert.Equal("BodyStarted,BodyFinished", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task FlushSendsHeaders()
        {
            ManualResetEvent block = new ManualResetEvent(false);
            var handler = new OwinClientHandler(env =>
            {
                IOwinContext context = new OwinContext(env);
                context.Response.Headers["TestHeader"] = "TestValue";
                context.Response.Body.Flush();
                block.WaitOne();
                context.Response.Write("BodyFinished");
                return Task.FromResult(0);
            });
            var httpClient = new HttpClient(handler);
            HttpResponseMessage response = await httpClient.GetAsync("https://example.com/",
                HttpCompletionOption.ResponseHeadersRead);
            Assert.Equal("TestValue", response.Headers.GetValues("TestHeader").First());
            block.Set();
            Assert.Equal("BodyFinished", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task ClientDisposalCloses()
        {
            ManualResetEvent block = new ManualResetEvent(false);
            var handler = new OwinClientHandler(env =>
            {
                IOwinContext context = new OwinContext(env);
                context.Response.Headers["TestHeader"] = "TestValue";
                context.Response.Body.Flush();
                block.WaitOne();
                return Task.FromResult(0);
            });
            var httpClient = new HttpClient(handler);
            HttpResponseMessage response = await httpClient.GetAsync("https://example.com/",
                HttpCompletionOption.ResponseHeadersRead);
            Assert.Equal("TestValue", response.Headers.GetValues("TestHeader").First());
            Stream responseStream = await response.Content.ReadAsStreamAsync();
            Task<int> readTask = responseStream.ReadAsync(new byte[100], 0, 100);
            Assert.False(readTask.IsCompleted);
            responseStream.Dispose();
            Thread.Sleep(50);
            Assert.True(readTask.IsCompleted);
            Assert.Equal(0, readTask.Result);
            block.Set();
        }

        [Fact]
        public async Task ClientCancellationAborts()
        {
            ManualResetEvent block = new ManualResetEvent(false);
            var handler = new OwinClientHandler(env =>
            {
                IOwinContext context = new OwinContext(env);
                context.Response.Headers["TestHeader"] = "TestValue";
                context.Response.Body.Flush();
                block.WaitOne();
                return Task.FromResult(0);
            });
            var httpClient = new HttpClient(handler);
            HttpResponseMessage response = await httpClient.GetAsync("https://example.com/",
                HttpCompletionOption.ResponseHeadersRead);
            Assert.Equal("TestValue", response.Headers.GetValues("TestHeader").First());
            Stream responseStream = await response.Content.ReadAsStreamAsync();
            CancellationTokenSource cts = new CancellationTokenSource();
            Task<int> readTask = responseStream.ReadAsync(new byte[100], 0, 100, cts.Token);
            Assert.False(readTask.IsCompleted);
            cts.Cancel();
            Thread.Sleep(50);
            Assert.True(readTask.IsCompleted);
            Assert.True(readTask.IsFaulted);
            block.Set();
        }

        [Fact]
        public void ExceptionBeforeFirstWriteIsReported()
        {
            var handler = new OwinClientHandler(env =>
            {
                throw new InvalidOperationException("Test Exception");
            });
            var httpClient = new HttpClient(handler);
            AggregateException ex = Assert.Throws<AggregateException>(() => httpClient.GetAsync("https://example.com/",
                HttpCompletionOption.ResponseHeadersRead).Result);
            Assert.IsType<InvalidOperationException>(ex.InnerException);
        }

        [Fact]
        public async Task ExceptionAfterFirstWriteIsReported()
        {
            ManualResetEvent block = new ManualResetEvent(false);
            var handler = new OwinClientHandler(env =>
            {
                IOwinContext context = new OwinContext(env);
                context.Response.Headers["TestHeader"] = "TestValue";
                context.Response.Write("BodyStarted");
                block.WaitOne();
                throw new InvalidOperationException("Test Exception");
            });
            var httpClient = new HttpClient(handler);
            HttpResponseMessage response = await httpClient.GetAsync("https://example.com/",
                HttpCompletionOption.ResponseHeadersRead);
            Assert.Equal("TestValue", response.Headers.GetValues("TestHeader").First());
            block.Set();
            AggregateException ex = Assert.Throws<AggregateException>(() => response.Content.ReadAsStringAsync().Result);
            Assert.True(ex.ToString().Contains("Test Exception"));
        }
    }
}
