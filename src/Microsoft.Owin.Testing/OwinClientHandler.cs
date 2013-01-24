using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Owin.Types;

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
            var owinRequest = OwinRequest.Create();
            owinRequest.Scheme = request.RequestUri.Scheme;
            owinRequest.Method = request.Method.ToString();
            owinRequest.Path = request.RequestUri.AbsolutePath;
            owinRequest.QueryString = request.RequestUri.Query;
            owinRequest.CallCancelled = cancellationToken;
            foreach (var header in request.Headers)
            {
                owinRequest.AddHeaderUnmodified(header.Key, header.Value);
            }
            var requestContent = request.Content;
            if (requestContent != null)
            {
                foreach (var header in request.Content.Headers)
                {
                    owinRequest.AddHeaderUnmodified(header.Key, header.Value);
                }
            }
            else
            {
                requestContent = new StreamContent(Stream.Null);
            }

            return requestContent.ReadAsStreamAsync()
                .Then(requestBody =>
                {
                    owinRequest.Body = requestBody;
                    var responseMemoryStream = new MemoryStream();

                    var owinResponse = new OwinResponse(owinRequest)
                    {
                        Body = responseMemoryStream
                    };

                    return _invoke.Invoke(owinRequest.Dictionary)
                        .Then(() =>
                        {
                            var response = new HttpResponseMessage();
                            response.StatusCode = (HttpStatusCode)owinResponse.StatusCode;
                            response.ReasonPhrase = owinResponse.ReasonPhrase;
                            response.RequestMessage = request;
                            //response.Version = owinResponse.Protocol;

                            responseMemoryStream.Seek(0, SeekOrigin.Begin);
                            response.Content = new StreamContent(responseMemoryStream);
                            foreach (var header in owinResponse.Headers)
                            {
                                if (!response.Headers.TryAddWithoutValidation(header.Key, header.Value))
                                {
                                    response.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                                }
                            }
                            return response;
                        });
                });
        }
    }
}