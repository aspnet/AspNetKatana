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
            CancellationToken token = Get<CancellationToken>(call.Environment, "host.CancellationToken");
            if (token == CancellationToken.None)
            {
                Task task = Get<Task>(call.Environment, "owin.CallCompleted");
                if (task != null)
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    task.ContinueWith((t) => { cts.Cancel(); });
                    call.Environment["host.CancellationToken"] = cts.Token;
                    token = cts.Token;
                }
            }
            return token;
        }

        public static HttpRequestMessage GetRequestMessage(CallParameters call)
        {
            var env = call.Environment;
            var requestMessage = Get<HttpRequestMessage>(env, "System.Net.Http.HttpRequestMessage");
            if (requestMessage != null)
            {
                return requestMessage;
            }

            var requestScheme = Get<string>(env, "owin.RequestScheme");
            var requestMethod = Get<string>(env, "owin.RequestMethod");
            var requestPathBase = Get<string>(env, "owin.RequestPathBase");
            var requestPath = Get<string>(env, "owin.RequestPath");
            var requestQueryString = Get<string>(env, "owin.RequestQueryString");

            var requestHeaders = call.Headers;
            var requestBody = call.Body;

            // TODO: tunnel value, and/or use series of fallback rules for determining
            var host = "localhost";
            var port = 80;

            var uriBuilder = new UriBuilder(requestScheme, host, port, requestPathBase + requestPath);
            if (!string.IsNullOrEmpty(requestQueryString))
            {
                uriBuilder.Query = requestQueryString;
            }

            requestMessage = new HttpRequestMessage(new HttpMethod(requestMethod), uriBuilder.Uri);
            requestMessage.Properties["OwinCall"] = call;
            requestMessage.Content = new StreamContent(requestBody ?? Stream.Null);

            foreach (var kv in requestHeaders)
            {
                if (!requestMessage.Headers.TryAddWithoutValidation(kv.Key,kv.Value))
                {
                    requestMessage.Content.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                }
            }
            env["owin.RequestHeaders"] = new RequestHeadersWrapper(requestMessage);
            env["System.Net.Http.HttpRequestMessage"] = requestMessage;
            return requestMessage;
        }

        public static CallParameters GetOwinCall(HttpRequestMessage request)
        {
            var owincall = Get<CallParameters>(request.Properties, "OwinCall");
            if (owincall.Environment != null)
            {
                return owincall;
            }

            throw new InvalidOperationException("Running OWIN components over a Web API server is not supported");
        }

        public static HttpResponseMessage GetResponseMessage(CallParameters call, ResultParameters result)
        {
            var responseMessage = Get<HttpResponseMessage>(call.Environment, "System.Net.Http.HttpResponseMessage");
            if (responseMessage != null)
            {
                return responseMessage;
            }

            var request = GetRequestMessage(call);

            int statusCode = result.Status;

            var message = new HttpResponseMessage((HttpStatusCode)statusCode)
                              {
                                  RequestMessage = request,
                                  Content = new BodyDelegateWrapper(result.Body)// GetCancellationToken(call.Environment))
                              };

            // TODO: Reason Phrase
            /*
            if (status != null && status.Length > 4)
            {
                message.ReasonPhrase = status.Substring(4);
            }*/

            foreach (var kv in result.Headers)
            {
                if (!message.Headers.TryAddWithoutValidation(kv.Key, kv.Value))
                {
                    message.Content.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                }
            }

            return message;
        }
    }
}
