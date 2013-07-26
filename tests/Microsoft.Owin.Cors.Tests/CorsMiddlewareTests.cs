// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Cors;
using Microsoft.Owin.Builder;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Cors.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class CorsMiddlewareTests
    {
        [Fact]
        public void Invoke_DoesNotAddHeaders_WhenOriginIsMissing()
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseCors(CorsOptions.AllowAll);

            var app = (AppFunc)builder.Build(typeof(AppFunc));

            OwinRequest request = CreateRequest("http://localhost/sample");
            app(request.Environment).Wait();

            var response = new OwinResponse(request.Environment);
            Assert.Empty(response.Headers);
        }

        [Theory]
        [InlineData("*", "*")]
        [InlineData("http://example.com", "http://example.com")]
        public void SendAsync_ReturnsAllowAOrigin(string policyOrigin, string expectedOrigin)
        {
            IAppBuilder builder = new AppBuilder();
            var policy = new CorsPolicy();

            if (policyOrigin == "*")
            {
                policy.AllowAnyOrigin = true;
            }
            else
            {
                policy.Origins.Add(policyOrigin);
            }

            builder.UseCors(new CorsOptions
            {
                CorsPolicy = policy
            });

            var app = (AppFunc)builder.Build(typeof(AppFunc));

            OwinRequest request = CreateRequest("http://localhost/sample");
            request.Headers.Set(CorsConstants.Origin, "http://example.com");
            app(request.Environment).Wait();

            var response = new OwinResponse(request.Environment);
            string origin = response.Headers.Get("Access-Control-Allow-Origin");

            Assert.Equal(expectedOrigin, origin);
        }

        [Theory]
        [InlineData("*", "DELETE", "*", "foo,bar")]
        [InlineData("http://example.com, http://localhost", "PUT", "http://localhost", "content-type,custom")]
        public void SendAsync_Preflight_ReturnsAllowMethodsAndAllowHeaders(string policyOrigin, string requestedMethod, string expectedOrigin, string requestedHeaders)
        {
            IAppBuilder builder = new AppBuilder();
            var policy = new CorsPolicy
            {
                AllowAnyHeader = true,
                AllowAnyMethod = true
            };

            if (policyOrigin == "*")
            {
                policy.AllowAnyOrigin = true;
            }
            else
            {
                foreach (var o in policyOrigin.Split(','))
                {
                    policy.Origins.Add(o.Trim());
                }
            }

            builder.UseCors(new CorsOptions
            {
                CorsPolicy = policy
            });

            var app = (AppFunc)builder.Build(typeof(AppFunc));

            OwinRequest request = CreateRequest("http://localhost/sample");
            request.Method = "OPTIONS";
            request.Headers.Set(CorsConstants.Origin, "http://localhost");
            request.Headers.Set(CorsConstants.AccessControlRequestMethod, requestedMethod);
            request.Headers.Set(CorsConstants.AccessControlRequestHeaders, requestedHeaders);
            app(request.Environment).Wait();

            var response = new OwinResponse(request.Environment);
            string origin = response.Headers.Get(CorsConstants.AccessControlAllowOrigin);
            string allowMethod = response.Headers.Get(CorsConstants.AccessControlAllowMethods);
            string[] allowHeaders = response.Headers.Get(CorsConstants.AccessControlAllowHeaders).Split(',');
            string[] requestedHeaderArray = requestedHeaders.Split(',');

            Assert.Equal(200, response.StatusCode);
            Assert.Equal(expectedOrigin, origin);
            Assert.Equal(requestedMethod, allowMethod);
            foreach (var requestedHeader in requestedHeaderArray)
            {
                Assert.Contains(requestedHeader, allowHeaders);
            }
        }

        [Fact]
        public void SendAsync_Preflight_ReturnsBadRequest_WhenOriginIsNotAllowed()
        {
            IAppBuilder builder = new AppBuilder();
            var policy = new CorsPolicy();
            policy.AllowAnyMethod = true;
            policy.AllowAnyHeader = true;
            policy.Origins.Add("http://www.example.com");
            builder.UseCors(new CorsOptions
            {
                CorsPolicy = policy
            });

            var app = (AppFunc)builder.Build(typeof(AppFunc));

            OwinRequest request = CreateRequest("http://localhost/default");
            request.Method = "OPTIONS";
            request.Headers.Set(CorsConstants.Origin, "http://localhost");
            request.Headers.Set(CorsConstants.AccessControlRequestMethod, "POST");
            app(request.Environment).Wait();

            var response = new OwinResponse(request.Environment);

            Assert.Equal(400, response.StatusCode);
        }

        private OwinRequest CreateRequest(string url)
        {
            var uriBuilder = new UriBuilder(url);
            var request = new OwinRequest();
            request.Path = uriBuilder.Path;
            request.PathBase = String.Empty;
            request.Scheme = uriBuilder.Scheme;
            request.QueryString = !String.IsNullOrEmpty(uriBuilder.Query) ? uriBuilder.Query.Substring(1) : String.Empty;

            return request;
        }
    }
}
