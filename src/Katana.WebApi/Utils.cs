using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
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

        public static CancellationToken GetCancellationToken(IDictionary<string, object> env)
        {
            return Get<CancellationToken>(env, "host.CallDisposed");
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
            requestMessage.Properties["OwinEnvironment"] = env;
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

        public static IDictionary<string, object> GetOwinEnvironment(HttpRequestMessage request)
        {
            var owinEnvironment = Get<IDictionary<string, object>>(request.Properties, "OwinEnvironment");
            if (owinEnvironment != null)
            {
                return owinEnvironment;
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
                                  Content = new BodyDelegateWrapper(result.Body, call.Completed)// GetCancellationToken(call.Environment))
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
