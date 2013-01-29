// <copyright file="OwinHttpListenerRequest.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
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
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Owin.Host.HttpListener.RequestProcessing
{
    /// <summary>
    /// This wraps an HttpListenerRequest and exposes it as an OWIN environment IDictionary.
    /// </summary>
    internal class OwinHttpListenerRequest
    {
        private readonly CallEnvironment _environment;
        private readonly HttpListenerRequest _request;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinHttpListenerRequest"/> class.
        /// Uses the given request object to populate the OWIN standard keys in the environment IDictionary.
        /// Most values are copied so that they can be mutable, but the headers collection is only wrapped.
        /// </summary>
        internal OwinHttpListenerRequest(HttpListenerRequest request, string basePath, string path, string query, CallEnvironment environment)
        {
            Contract.Requires(request != null);

            _request = request;
            _environment = environment;

            _environment.RequestProtocol = GetProtocol(request.ProtocolVersion);
            _environment.RequestScheme = request.Url.Scheme;
            _environment.RequestMethod = request.HttpMethod;
            _environment.RequestPathBase = basePath;
            _environment.RequestPath = path;
            _environment.RequestQueryString = query;

            _environment.RequestHeaders = new RequestHeadersDictionary(request);

            if (_request.IsSecureConnection)
            {
                // TODO: Add delay sync load for folks that directly access the client cert key
                _environment.LoadClientCert = (Func<Task>)LoadClientCertAsync;
            }
        }

        private static string GetProtocol(Version version)
        {
            if (version.Major == 1)
            {
                if (version.Minor == 1)
                {
                    return "HTTP/1.1";
                }
                else if (version.Minor == 0)
                {
                    return "HTTP/1.0";
                }
            }
            return "HTTP/" + version.ToString(2);
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
                return TaskHelpers.Completed();
            }
        }

        internal Stream GetRequestBody()
        {
            return new HttpListenerStreamWrapper(_request.InputStream);
        }

        internal string GetRemoteIpAddress()
        {
            return _request.RemoteEndPoint.Address.ToString();
        }

        internal string GetRemotePort()
        {
            return _request.RemoteEndPoint.Port.ToString(CultureInfo.InvariantCulture);
        }

        internal string GetLocalIpAddress()
        {
            return _request.LocalEndPoint.Address.ToString();
        }

        internal string GetLocalPort()
        {
            return _request.LocalEndPoint.Port.ToString(CultureInfo.InvariantCulture);
        }

        internal bool GetIsLocal()
        {
            return _request.IsLocal;
        }
    }
}
