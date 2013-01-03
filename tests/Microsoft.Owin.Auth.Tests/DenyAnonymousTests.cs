// -----------------------------------------------------------------------
// <copyright file="DenyAnonymousTests.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Owin.Auth.Tests
{
    public class DenyAnonymousTests
    {
        private const int DefaultStatusCode = 201;

        [Fact]
        public void DenyAnonymous_WithoutCredentials_401()
        {
            DenyAnonymousMiddleware denyAnon = new DenyAnonymousMiddleware(SimpleApp);
            IDictionary<string, object> emptyEnv = CreateEmptyRequest();
            denyAnon.Invoke(emptyEnv).Wait();

            Assert.Equal(401, emptyEnv.Get<int>("owin.ResponseStatusCode"));
            var responseHeaders = emptyEnv.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
            Assert.Equal(0, responseHeaders.Count);
        }

        [Fact]
        public void DenyAnonymous_WithCredentials_PassedThrough()
        {
            DenyAnonymousMiddleware denyAnon = new DenyAnonymousMiddleware(SimpleApp);
            IDictionary<string, object> emptyEnv = CreateEmptyRequest();
            emptyEnv["server.User"] = new GenericPrincipal(new GenericIdentity("bob"), null);
            denyAnon.Invoke(emptyEnv).Wait();

            Assert.Equal(DefaultStatusCode, emptyEnv.Get<int>("owin.ResponseStatusCode"));
            var responseHeaders = emptyEnv.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
            Assert.Equal(0, responseHeaders.Count);
        }

        private IDictionary<string, object> CreateEmptyRequest(string header = null, string value = null)
        {
            IDictionary<string, object> env = new Dictionary<string, object>();
            var requestHeaders = new Dictionary<string, string[]>();
            env["owin.RequestHeaders"] = requestHeaders;
            if (header != null)
            {
                requestHeaders[header] = new string[] { value };
            }
            env["owin.ResponseHeaders"] = new Dictionary<string, string[]>();
            return env;
        }

        private Task SimpleApp(IDictionary<string, object> env)
        {
            env["owin.ResponseStatusCode"] = DefaultStatusCode;
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.TrySetResult(null);
            return tcs.Task;
        }
    }
}
