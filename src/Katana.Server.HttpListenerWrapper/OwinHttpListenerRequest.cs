using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Owin;

namespace Katana.Server.HttpListenerWrapper
{
    internal class OwinHttpListenerRequest
    {
        private IDictionary<string, object> environment;
        private HttpListenerRequest request;
        private BodyDelegate bodyDelegate;
        IDictionary<string, string[]> headers;

        public IDictionary<string, object> Environment { get { return environment; } }
        
        internal OwinHttpListenerRequest(HttpListenerRequest request)
        {
            Debug.Assert(request != null);
            this.request = request;

            environment = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            environment.Add(Constants.VersionKey, Constants.OwinVersion);
            environment.Add(Constants.HttpVersionKey, "HTTP/" + request.ProtocolVersion.ToString());
            environment.Add(Constants.RequestSchemeKey, request.Url.Scheme);
            environment.Add(Constants.RequestMethodKey, request.HttpMethod);
            environment.Add(Constants.RequestPathBaseKey, string.Empty); // TODO: This should be set by the OwinHttpListener, not per request
            environment.Add(Constants.RequestPathKey, request.Url.AbsolutePath); // TODO: must be relative to the base path

            string query = request.Url.Query;
            if (query.StartsWith("?")) // Trim '?'
            {
                query = query.Substring(1);
            }
            environment.Add(Constants.RequestQueryStringKey, query);

            headers = new NameValueToDictionaryWrapper(request.Headers);
            environment.Add(Constants.RequestHeadersKey, headers);

            if (!request.HttpMethod.Equals("HEAD", StringComparison.OrdinalIgnoreCase) 
                && request.ContentLength64 != 0) // -1 for chunked/unknown
            {
                bodyDelegate = ProcessRequestBody;
            }
            environment.Add(Constants.RequestBodyKey, bodyDelegate); // May be null
        }

        private void ProcessRequestBody(Func<ArraySegment<byte>, Action, bool> write, 
            Action<Exception> end, CancellationToken cancellationToken)
        {
            Helpers.CopyFromStreamToOwin(request.InputStream, write, end, cancellationToken);
        }
    }
}
