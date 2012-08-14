using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Katana.WebApi.CallContent;
using Katana.WebApi.CallHeaders;
using Owin;

namespace Katana.WebApi
{
    public static class Utils
    {
        internal static T Get<T>(IDictionary<string, object> env, string key)
        {
            object value;
            if (env.TryGetValue(key, out value))
            {
                return (T)value;
            }
            return default(T);
        }

        public static CancellationToken GetCancellationToken(CallParameters call)
        {
            CancellationToken token = Get<CancellationToken>(call.Environment, "webapi.CancellationToken");
            if (token == CancellationToken.None)
            {
                Task task = Get<Task>(call.Environment, "owin.CallCompleted");
                if (task != null)
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    task.ContinueWith((t) => { cts.Cancel(); });
                    call.Environment["webapi.CancellationToken"] = cts.Token;
                    token = cts.Token;
                }
            }
            return token;
        }

        public static HttpRequestMessage GetRequestMessage(CallParameters call)
        {
            var requestHeadersWrapper = call.Headers as RequestHeadersWrapper;
            var requestMessage = requestHeadersWrapper != null ? requestHeadersWrapper.Message : null;
            if (requestMessage == null)
            {
                // initial transition to HRM, or headers dictionary has been substituted
                var requestScheme = Utils.Get<string>(call.Environment, "owin.RequestScheme");
                var requestMethod = Utils.Get<string>(call.Environment, "owin.RequestMethod");
                var requestPathBase = Utils.Get<string>(call.Environment, "owin.RequestPathBase");
                var requestPath = Utils.Get<string>(call.Environment, "owin.RequestPath");
                var requestQueryString = Utils.Get<string>(call.Environment, "owin.RequestQueryString");

                // default values, in absense of a host header
                var host = "127.0.0.1";
                var port = String.Equals(requestScheme, "https", StringComparison.OrdinalIgnoreCase) ? 443 : 80;

                // if a single host header is available
                string[] hostAndPort;
                if (call.Headers.TryGetValue("Host", out hostAndPort) &&
                    hostAndPort != null &&
                    hostAndPort.Length == 1 &&
                    !String.IsNullOrWhiteSpace(hostAndPort[0]))
                {
                    // try to parse as "host:port" format
                    var delimiterIndex = hostAndPort[0].LastIndexOf(':');
                    int portValue;
                    if (delimiterIndex != -1 &&
                        Int32.TryParse(hostAndPort[0].Substring(delimiterIndex + 1), out portValue))
                    {
                        // use those two values
                        host = hostAndPort[0].Substring(0, delimiterIndex);
                        port = portValue;
                    }
                    else
                    {
                        // otherwise treat as host name
                        host = hostAndPort[0];
                    }
                }

                var uriBuilder = new UriBuilder(requestScheme, host, port, requestPathBase + requestPath);
                if (!String.IsNullOrEmpty(requestQueryString))
                {
                    uriBuilder.Query = requestQueryString;
                }

                requestMessage = new HttpRequestMessage(new HttpMethod(requestMethod), uriBuilder.Uri);
                call.Environment["System.Net.Http.HttpRequestMessage"] = requestMessage;

                requestMessage.Content = new BodyStreamContent(call.Body ?? Stream.Null);

                foreach (var kv in call.Headers)
                {
                    if (!requestMessage.Headers.TryAddWithoutValidation(kv.Key, kv.Value))
                    {
                        requestMessage.Content.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                    }
                }
            }

            var bodyStreamContent = requestMessage.Content as BodyStreamContent;

            var sameBody =
                (bodyStreamContent != null && ReferenceEquals(bodyStreamContent.Body, call.Body));

            if (!sameBody)
            {
                // body stream has been substituted
                var newBodyContent = new BodyStreamContent(call.Body ?? Stream.Null);
                if (requestMessage.Content != null)
                {
                    foreach (var kv in requestMessage.Content.Headers)
                    {
                        newBodyContent.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                    }
                }
                requestMessage.Content = newBodyContent;
            }

            requestMessage.Properties["OwinEnvironment"] = call.Environment;
            return requestMessage;
        }

        public static Task<CallParameters> GetCallParameters(HttpRequestMessage request)
        {
            var call = new CallParameters
            {
                Environment = Utils.Get<IDictionary<string, object>>(request.Properties, "OwinEnvironment"),
                Headers = new RequestHeadersWrapper(request),
            };

            if (call.Environment == null)
            {
                throw new InvalidOperationException("Running OWIN components over a Web API server is not currently supported");
            }

            var bodyStreamContent = request.Content as BodyStreamContent;

            if (bodyStreamContent != null)
            {
                // Content stream is the same as before
                call.Body = bodyStreamContent.Body;
                return TaskHelpers.FromResult(call);
            }
            else if (request.Content != null)
            {
                // Request content is substitiuted - re-acquire as new stream
                return request.Content.ReadAsStreamAsync()
                    .Then(stream =>
                    {
                        call.Body = stream;
                        return call;
                    });
            }
            else
            {
                // Request content was set to null - allow call.Body to remain null
                return TaskHelpers.FromResult(call);
            }
        }

        public static HttpResponseMessage GetResponseMessage(CallParameters call, ResultParameters result)
        {
            var request = GetRequestMessage(call);

            int statusCode = result.Status;

            var message = new HttpResponseMessage((HttpStatusCode)statusCode)
                              {
                                  RequestMessage = request,
                                  Content = new BodyDelegateWrapper(result.Body)
                              };

            object reasonPhrase;
            if (result.Properties != null && result.Properties.TryGetValue("owin.ReasonPhrase", out reasonPhrase))
            {
                message.ReasonPhrase = Convert.ToString(reasonPhrase);
            }

            foreach (var kv in result.Headers)
            {
                if (!message.Headers.TryAddWithoutValidation(kv.Key, kv.Value))
                {
                    message.Content.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                }
            }

            return message;
        }

        public static ResultParameters GetResultParameters(HttpResponseMessage responseMessage)
        {
            return new ResultParameters
            {
                Status = (int)responseMessage.StatusCode,
                Headers = new ResponseHeadersWrapper(responseMessage),
                Body = stream => responseMessage.Content == null ? TaskHelpers.Completed() : responseMessage.Content.CopyToAsync(stream),
                Properties = new Dictionary<string, object>
                {
                    { "owin.ReasonPhrase", responseMessage.ReasonPhrase } 
                },
            };
        }
    }
}
