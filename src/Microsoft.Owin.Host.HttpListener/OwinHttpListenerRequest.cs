// <copyright file="OwinHttpListenerRequest.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Microsoft.Owin.Host.HttpListener
{
    /// <summary>
    /// This wraps an HttpListenerRequest and exposes it as an OWIN environment IDictionary.
    /// </summary>
    internal class OwinHttpListenerRequest
    {
        private readonly IDictionary<string, object> _environment;
        private HttpListenerRequest _request;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinHttpListenerRequest"/> class.
        /// Uses the given request object to populate the OWIN standard keys in the environment IDictionary.
        /// Most values are copied so that they can be mutable, but the headers collection is only wrapped.
        /// </summary>
        /// <param name="request">The request to expose in the OWIN environment.</param>
        /// <param name="basePath">The base server path accepting requests.</param>
        internal OwinHttpListenerRequest(HttpListenerRequest request, string basePath)
        {
            Contract.Requires(request != null);
            Contract.Requires(request.Url.AbsolutePath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase));

            _request = request;
            _environment = new Dictionary<string, object>();

            _environment.Add(Constants.HttpRequestProtocolKey, "HTTP/" + request.ProtocolVersion.ToString(2));
            _environment.Add(Constants.RequestSchemeKey, request.Url.Scheme);
            _environment.Add(Constants.RequestMethodKey, request.HttpMethod);
            _environment.Add(Constants.RequestPathBaseKey, basePath);

            // Path is relative to the server base path.
            string path = request.Url.AbsolutePath.Substring(basePath.Length);
            _environment.Add(Constants.RequestPathKey, path);

            string query = request.Url.Query;
            if (query.StartsWith("?", StringComparison.Ordinal))
            {
                query = query.Substring(1);
            }

            _environment.Add(Constants.RequestQueryStringKey, query);

            _environment.Add(Constants.RequestBodyKey, new HttpListenerStreamWrapper(request.InputStream));
            _environment.Add(Constants.RequestHeadersKey, new RequestHeadersDictionary(request.Headers));

            if (_request.IsSecureConnection)
            {
                // TODO: Add delay sync load for folks that directly access the client cert key
                _environment.Add(Constants.LoadClientCertAsyncKey, (Func<Task>)LoadClientCertAsync);
            }

            _environment.Add(Constants.RemoteIpAddressKey, request.RemoteEndPoint.Address.ToString());
            _environment.Add(Constants.RemotePortKey, request.RemoteEndPoint.Port.ToString(CultureInfo.InvariantCulture));
            _environment.Add(Constants.LocalIpAddressKey, request.LocalEndPoint.Address.ToString());
            _environment.Add(Constants.LocalPortKey, request.LocalEndPoint.Port.ToString(CultureInfo.InvariantCulture));
            _environment.Add(Constants.IsLocalKey, request.IsLocal);
        }

        public IDictionary<string, object> Environment
        {
            get { return _environment; }
        }

        private Task LoadClientCertAsync()
        {
            try
            {
                // TODO: Check request.ClientCertificateError if clientCert is null?
                return _request.GetClientCertificateAsync()
                    .Then(cert => _environment[Constants.ClientCertifiateKey] = cert)
                    .Catch(errorInfo =>
                    {
                        // TODO: LOG
                        return errorInfo.Handled();
                    });
            }
            catch (HttpListenerException)
            {
                // TODO: LOG
            }
            return TaskHelpers.Completed();
        }
    }
}
