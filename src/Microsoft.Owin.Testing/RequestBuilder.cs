// <copyright file="RequestBuilder.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Owin.Testing
{
    public class RequestBuilder : IDisposable
    {
        private readonly TestServer _server;
        private readonly HttpRequestMessage _req;

        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "Not a full URI")]
        public RequestBuilder(TestServer server, string path)
        {
            _server = server;
            _req = new HttpRequestMessage(HttpMethod.Get, "http://localhost" + path);
        }

        public RequestBuilder And(Action<HttpRequestMessage> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException("configure");
            }

            configure(_req);
            return this;
        }

        public RequestBuilder Header(string name, string value)
        {
            if (!_req.Headers.TryAddWithoutValidation(name, value))
            {
                _req.Content.Headers.TryAddWithoutValidation(name, value);
            }
            return this;
        }

        public Task<HttpResponseMessage> SendAsync(string method)
        {
            _req.Method = new HttpMethod(method);
            return _server.HttpClient.SendAsync(_req);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _req.Dispose();
            }
        }
    }
}
