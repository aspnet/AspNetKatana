//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Katana.Server.HttpListenerWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Owin;

    /// <summary>
    /// This wraps an HttpListenerRequest and exposes it as an OWIN environment IDictionary.
    /// </summary>
    internal class OwinHttpListenerRequest : IDisposable
    {
        private IDictionary<string, object> environment;
        private HttpListenerRequest request;
        private Stream body;
        private IDictionary<string, string[]> headers;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinHttpListenerRequest"/> class.
        /// Uses the given request object to populate the OWIN standard keys in the environment IDictionary.
        /// Most values are copied so that they can be mutable, but the headers collection is only wrapped.
        /// </summary>
        /// <param name="request">The request to expose in the OWIN environment.</param>
        /// <param name="basePath">The base server path accepting requests.</param>
        /// <param name="clientCert">The client certificate provided, if any.</param>
        internal OwinHttpListenerRequest(HttpListenerRequest request, string basePath, X509Certificate2 clientCert)
        {
            Contract.Requires(request != null);
            Contract.Requires(request.Url.AbsolutePath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase));

            this.request = request;
            this.environment = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            this.environment.Add(Constants.VersionKey, Constants.OwinVersion);
            this.environment.Add(Constants.HttpRequestProtocolKey, "HTTP/" + request.ProtocolVersion.ToString(2));
            this.environment.Add(Constants.RequestSchemeKey, request.Url.Scheme);
            this.environment.Add(Constants.RequestMethodKey, request.HttpMethod);
            this.environment.Add(Constants.RequestPathBaseKey, basePath);

            // Path is relative to the server base path.
            string path = request.Url.AbsolutePath.Substring(basePath.Length);
            this.environment.Add(Constants.RequestPathKey, path);
            
            string query = request.Url.Query;
            if (query.StartsWith("?"))
            {
                query = query.Substring(1);
            }

            this.environment.Add(Constants.RequestQueryStringKey, query);

            this.headers = new NameValueToDictionaryWrapper(request.Headers);
            
            // ContentLength64 returns -1 for chunked or unknown
            if (!request.HttpMethod.Equals("HEAD", StringComparison.OrdinalIgnoreCase) && request.ContentLength64 != 0)
            {
                this.body = new HttpListenerStreamWrapper(request.InputStream);
            }

            if (clientCert != null)
            {
                this.environment.Add(Constants.ClientCertifiateKey, clientCert);
            }

            this.environment.Add(Constants.RemoteEndPointKey, request.RemoteEndPoint.ToString());
            this.environment.Add(Constants.LocalEndPointKey, request.LocalEndPoint.ToString());
            this.environment.Add(Constants.IsLocalKey, request.IsLocal);
        }

        public CallParameters AppParameters
        {
            get
            {
                return new CallParameters()
                {
                    Environment = this.environment,
                    Headers = this.headers,
                    Body = this.body,
                };
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.body != null)
                {
                    this.body.Dispose();
                }
            }
        }
    }
}
