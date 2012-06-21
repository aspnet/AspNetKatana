namespace Katana.Server.HttpListenerWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Net;
    using System.Threading;
    using Owin;

    /// <summary>
    /// This wraps an HttpListenerRequest and exposes it as an OWIN environment IDictionary.
    /// </summary>
    internal class OwinHttpListenerRequest
    {
        private IDictionary<string, object> environment;
        private HttpListenerRequest request;
        private BodyDelegate bodyDelegate;
        private IDictionary<string, string[]> headers;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinHttpListenerRequest"/> class.
        /// Uses the given request object to populate the OWIN standard keys in the environment IDictionary.
        /// Most values are copied so that they can be mutable, but the headers collection is only wrapped.
        /// </summary>
        /// <param name="request">The request to expose in the OWIN environment.</param>
        internal OwinHttpListenerRequest(HttpListenerRequest request)
        {
            Contract.Requires(request != null);
            this.request = request;

            this.environment = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            this.environment.Add(Constants.VersionKey, Constants.OwinVersion);
            this.environment.Add(Constants.HttpRequestProtocolKey, "HTTP/" + request.ProtocolVersion.ToString());
            this.environment.Add(Constants.RequestSchemeKey, request.Url.Scheme);
            this.environment.Add(Constants.RequestMethodKey, request.HttpMethod);
            this.environment.Add(Constants.RequestPathBaseKey, string.Empty); // TODO: This should be set by the OwinHttpListener, not per request
            this.environment.Add(Constants.RequestPathKey, request.Url.AbsolutePath); // TODO: must be relative to the base path

            string query = request.Url.Query;
            if (query.StartsWith("?"))
            {
                query = query.Substring(1); // Trim leading '?'
            }

            this.environment.Add(Constants.RequestQueryStringKey, query);

            this.headers = new NameValueToDictionaryWrapper(request.Headers);
            this.environment.Add(Constants.RequestHeadersKey, this.headers);

            // ContentLength64 returns -1 for chunked or unknown
            if (!request.HttpMethod.Equals("HEAD", StringComparison.OrdinalIgnoreCase) && request.ContentLength64 != 0)
            {
                this.bodyDelegate = this.ProcessRequestBody;
            }

            this.environment.Add(Constants.RequestBodyKey, this.bodyDelegate); // May be null
        }

        public IDictionary<string, object> Environment 
        { 
            get 
            { 
                return this.environment; 
            } 
        }

        private void ProcessRequestBody(
            Func<ArraySegment<byte>, Action, bool> write, 
            Action<Exception> end, 
            CancellationToken cancellationToken)
        {
            Helpers.CopyFromStreamToOwin(this.request.InputStream, write, end, cancellationToken);
        }
    }
}
