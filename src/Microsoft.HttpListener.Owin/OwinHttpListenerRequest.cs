//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.HttpListener.Owin
{
    /// <summary>
    /// This wraps an HttpListenerRequest and exposes it as an OWIN environment IDictionary.
    /// </summary>
    internal class OwinHttpListenerRequest
    {
        private IDictionary<string, object> environment;
        private HttpListenerRequest request;

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
            this.environment = new Dictionary<string, object>();

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

            this.environment.Add(Constants.RequestBodyKey, new HttpListenerStreamWrapper(request.InputStream));
            this.environment.Add(Constants.RequestHeadersKey, new RequestHeadersDictionary(request.Headers));

            if (clientCert != null)
            {
                this.environment.Add(Constants.ClientCertifiateKey, clientCert);
            }

            this.environment.Add(Constants.RemoteIpAddressKey, request.RemoteEndPoint.Address.ToString());
            this.environment.Add(Constants.RemotePortKey, request.RemoteEndPoint.Port.ToString(CultureInfo.InvariantCulture));
            this.environment.Add(Constants.LocalIpAddressKey, request.LocalEndPoint.Address.ToString());
            this.environment.Add(Constants.LocalPortKey, request.LocalEndPoint.Port.ToString(CultureInfo.InvariantCulture));
            this.environment.Add(Constants.IsLocalKey, request.IsLocal);
        }

        public IDictionary<string, object> Environment
        {
            get { return environment; }
        }
    }
}
