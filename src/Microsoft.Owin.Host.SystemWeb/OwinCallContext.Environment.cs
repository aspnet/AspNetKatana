// <copyright file="OwinCallContext.Environment.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
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
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;
using Microsoft.Owin.Host.SystemWeb.CallHeaders;
using Microsoft.Owin.Host.SystemWeb.CallStreams;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal partial class OwinCallContext : AspNetDictionary.IPropertySource
    {
        public void CreateEnvironment()
        {
            // Mitigate double registered modules, and also allow the env to be shared between integrated pipeline and
            // owin based handlers.
            if (_httpContext.Items.Contains(HttpContextItemKeys.OwinEnvironmentKey))
            {
                _env = _httpContext.Items[HttpContextItemKeys.OwinEnvironmentKey] as AspNetDictionary;
                System.Diagnostics.Debug.Assert(_env != null, "Environment type mismatch, " 
                    + _httpContext.Items[HttpContextItemKeys.OwinEnvironmentKey]);
                return;
            }

            // Note, simple or expensive fields are delay loaded internally.
            // e.g. the first access to _httpRequest.ServerVariables[...] is extremely slow
            _env = new AspNetDictionary(this);

            _env.OwinVersion = Constants.OwinVersion;

            _env.RequestPathBase = _requestPathBase;
            _env.RequestPath = _requestPath;
            _env.RequestMethod = _httpRequest.HttpMethod;
            _env.RequestHeaders = new AspNetRequestHeaders(_httpRequest.Headers);
            _env.ResponseHeaders = new AspNetResponseHeaders(_httpResponse);

            _env.OnSendingHeaders = _sendingHeadersEvent.Register;
            _env.SendFileAsync = SendFileAsync;

            _env.HostTraceOutput = TraceTextWriter.Instance;
            _env.HostAppName = LazyInitializer.EnsureInitialized(ref _hostAppName,
                () => HostingEnvironment.SiteName ?? new Guid().ToString());

            _env.DisableResponseCompression = DisableResponseCompression;
            _env.ServerCapabilities = _appContext.Capabilities;

            _env.RequestContext = _requestContext;
            _env.HttpContextBase = _httpContext;

            _httpContext.Items[HttpContextItemKeys.OwinEnvironmentKey] = _env;
        }

        #region Implementation of IPropertySource

        CancellationToken AspNetDictionary.IPropertySource.GetOnAppDisposing()
        {
            return OwinApplication.ShutdownToken;
        }

        IPrincipal AspNetDictionary.IPropertySource.GetServerUser()
        {
            return _httpContext.User;
        }

        void AspNetDictionary.IPropertySource.SetServerUser(IPrincipal value)
        {
            _httpContext.User = value;
        }

        CancellationToken AspNetDictionary.IPropertySource.GetCallCancelled()
        {
            return BindDisconnectNotification();
        }

        string AspNetDictionary.IPropertySource.GetRequestProtocol()
        {
            return _httpRequest.ServerVariables["SERVER_PROTOCOL"];
        }

        string AspNetDictionary.IPropertySource.GetRequestScheme()
        {
            return _httpRequest.IsSecureConnection ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
        }

        string AspNetDictionary.IPropertySource.GetRequestQueryString()
        {
            string requestQueryString = String.Empty;
            Uri uri = _httpRequest.Url;
            if (uri != null)
            {
                string query = uri.Query + uri.Fragment;  // System.Uri mistakes un-escaped # in the query as a fragment
                if (query.Length > 1)
                {
                    // pass along the query string without the leading "?" character
                    requestQueryString = query.Substring(1);
                }
            }
            return requestQueryString;
        }

        Stream AspNetDictionary.IPropertySource.GetRequestBody()
        {
            // OFFLINE: facade? favor nonblocking option?
            return _httpRequest.InputStream;
        }

        int AspNetDictionary.IPropertySource.GetResponseStatusCode()
        {
            return _httpResponse.StatusCode;
        }

        void AspNetDictionary.IPropertySource.SetResponseStatusCode(int value)
        {
            _httpResponse.StatusCode = value;
        }

        string AspNetDictionary.IPropertySource.GetResponseReasonPhrase()
        {
            return _httpResponse.StatusDescription;
        }

        void AspNetDictionary.IPropertySource.SetResponseReasonPhrase(string value)
        {
            _httpResponse.StatusDescription = value;
        }

        Stream AspNetDictionary.IPropertySource.GetResponseBody()
        {
            return new OutputStream(_httpResponse, _httpResponse.OutputStream, OnStart, OnFaulted);
        }

        bool AspNetDictionary.IPropertySource.TryGetHostAppMode(ref string value)
        {
            if (_httpContext.IsDebuggingEnabled)
            {
                value = Constants.AppModeDevelopment;
                return true;
            }
            return false;
        }

        string AspNetDictionary.IPropertySource.GetServerRemoteIpAddress()
        {
            return _httpRequest.ServerVariables["REMOTE_ADDR"];
        }

        string AspNetDictionary.IPropertySource.GetServerRemotePort()
        {
            return _httpRequest.ServerVariables["REMOTE_PORT"];
        }

        string AspNetDictionary.IPropertySource.GetServerLocalIpAddress()
        {
            return _httpRequest.ServerVariables["LOCAL_ADDR"];
        }

        string AspNetDictionary.IPropertySource.GetServerLocalPort()
        {
            return _httpRequest.ServerVariables["SERVER_PORT"];
        }

        bool AspNetDictionary.IPropertySource.GetServerIsLocal()
        {
            return _httpRequest.IsLocal;
        }

        bool AspNetDictionary.IPropertySource.TryGetClientCert(ref X509Certificate value)
        {
            if (_httpContext.Request.IsSecureConnection)
            {
                try
                {
                    if (_httpContext.Request.ClientCertificate != null
                        && _httpContext.Request.ClientCertificate.IsPresent)
                    {
                        value = new X509Certificate2(_httpContext.Request.ClientCertificate.Certificate);
                        return true;
                    }
                }
                catch (CryptographicException ce)
                {
                    _trace.WriteError(Resources.Trace_ClientCertException, ce);
                }
            }
            return false;
        }

        bool AspNetDictionary.IPropertySource.TryGetLoadClientCert(ref Func<Task> value)
        {
            if (_httpContext.Request.IsSecureConnection)
            {
                value = LoadClientCertAsync;
                return true;
            }
            return false;
        }

        #endregion
    }
}
