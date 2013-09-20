// <copyright file="OwinClientHandler.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.Testing
{
    public class OwinClientHandler : HttpMessageHandler
    {
        private readonly Func<IDictionary<string, object>, Task> _invoke;

        public OwinClientHandler(Func<IDictionary<string, object>, Task> invoke)
        {
            _invoke = invoke;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var state = new RequestState(request, cancellationToken);
            HttpContent requestContent = request.Content ?? new StreamContent(Stream.Null);
            return requestContent.ReadAsStreamAsync().Then(
                body =>
                {
                    state.OwinContext.Request.Body = body;
                    return _invoke(state.Environment).Then(() => state.GenerateResponse());
                });
        }

        private class RequestState
        {
            private readonly HttpRequestMessage _request;
            private Action _sendingHeaders;

            internal RequestState(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                _request = request;
                _sendingHeaders = () => { };

                request.Headers.Host = request.RequestUri.GetComponents(UriComponents.HostAndPort | UriComponents.StrongPort, UriFormat.UriEscaped);

                OwinContext = new OwinContext();
                OwinContext.Set("owin.Version", "1.0");
                IOwinRequest owinRequest = OwinContext.Request;
                owinRequest.Protocol = "HTTP/" + request.Version.ToString(2);
                owinRequest.Scheme = request.RequestUri.Scheme;
                owinRequest.Method = request.Method.ToString();
                owinRequest.Path = PathString.FromUriComponent(request.RequestUri);
                owinRequest.PathBase = PathString.Empty;
                owinRequest.QueryString = QueryString.FromUriComponent(request.RequestUri);
                owinRequest.CallCancelled = cancellationToken;
                owinRequest.Set<Action<Action<object>, object>>("server.OnSendingHeaders", (callback, state) =>
                {
                    var prior = _sendingHeaders;
                    _sendingHeaders = () =>
                    {
                        prior();
                        callback(state);
                    };
                });

                foreach (var header in request.Headers)
                {
                    owinRequest.Headers.AppendValues(header.Key, header.Value.ToArray());
                }
                HttpContent requestContent = request.Content;
                if (requestContent != null)
                {
                    foreach (var header in request.Content.Headers)
                    {
                        owinRequest.Headers.AppendValues(header.Key, header.Value.ToArray());
                    }
                }
                else
                {
                    requestContent = new StreamContent(Stream.Null);
                }

                OwinContext.Response.Body = new MemoryStream();
                OwinContext.Response.StatusCode = 200;
            }

            public IOwinContext OwinContext { get; private set; }

            public IDictionary<string, object> Environment
            {
                get { return OwinContext.Environment; }
            }

            [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
                Justification = "HttpResposneMessage must be returned to the caller.")]
            internal HttpResponseMessage GenerateResponse()
            {
                _sendingHeaders();

                var response = new HttpResponseMessage();
                response.StatusCode = (HttpStatusCode)OwinContext.Response.StatusCode;
                response.ReasonPhrase = OwinContext.Response.ReasonPhrase;
                response.RequestMessage = _request;
                // response.Version = owinResponse.Protocol;

                OwinContext.Response.Body.Seek(0, SeekOrigin.Begin);
                response.Content = new StreamContent(OwinContext.Response.Body);
                foreach (var header in OwinContext.Response.Headers)
                {
                    if (!response.Headers.TryAddWithoutValidation(header.Key, header.Value))
                    {
                        response.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
                return response;
            }
        }
    }
}
