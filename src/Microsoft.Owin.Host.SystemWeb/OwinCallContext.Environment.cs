// <copyright file="OwinCallContext.Environment.cs" company="Katana contributors">
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
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Routing;
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;
using Microsoft.Owin.Host.SystemWeb.CallHeaders;
using Microsoft.Owin.Host.SystemWeb.CallStreams;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal partial class OwinCallContext : AspNetDictionary.IPropertySource
    {
        private void CreateEnvironment()
        {
            // Note, simple or expensive fields are delay loaded internally.
            // e.g. the first access to _httpRequest.ServerVariables[...] is extremely slow
            _env = new AspNetDictionary(this);

            _env.OnSendingHeaders = _sendingHeadersEvent.Register;
            _env.RequestPathBase = _requestPathBase;
            _env.RequestPath = _requestPath;
            _env.ResponseBody = new OutputStream(_httpResponse, _httpResponse.OutputStream, OnStart, OnFaulted);
            _env.SendFileAsync = SendFileAsync;
            _env.HostAppName = LazyInitializer.EnsureInitialized(ref _hostAppName,
                () => HostingEnvironment.SiteName ?? new Guid().ToString());
            _env.ServerDisableResponseBuffering = DisableResponseBuffering;
        }

        #region Implementation of IPropertySource

        string AspNetDictionary.IPropertySource.GetOwinVersion()
        {
            return "1.0";
        }

        CancellationToken AspNetDictionary.IPropertySource.GetCallCancelled()
        {
            return BindDisconnectNotification();
        }

        string AspNetDictionary.IPropertySource.GetRequestProtocol()
        {
            return _httpRequest.ServerVariables["SERVER_PROTOCOL"];
        }

        string AspNetDictionary.IPropertySource.GetRequestMethod()
        {
            return _httpRequest.HttpMethod;
        }

        string AspNetDictionary.IPropertySource.GetRequestScheme()
        {
            return _httpRequest.IsSecureConnection ? "https" : "http";
        }

        string AspNetDictionary.IPropertySource.GetRequestQueryString()
        {
            return GetQuery();
        }

        IDictionary<string, string[]> AspNetDictionary.IPropertySource.GetRequestHeaders()
        {
            return new AspNetRequestHeaders(_httpRequest.Headers);
        }

        IDictionary<string, string[]> AspNetDictionary.IPropertySource.GetResponseHeaders()
        {
            return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        }

        TextWriter AspNetDictionary.IPropertySource.GetHostTraceOutput()
        {
            return TraceTextWriter.Instance;
        }

        string AspNetDictionary.IPropertySource.GetHostAppMode()
        {
            return GetAppMode();
        }

        IPrincipal AspNetDictionary.IPropertySource.GetServerUser()
        {
            return _httpContext.User;
        }

        IDictionary<string, object> AspNetDictionary.IPropertySource.GetServerCapabilities()
        {
            return _appContext.Capabilities;
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
                    Trace.WriteLine(Resources.Exception_ClientCert);
                    Trace.WriteLine(ce.ToString());
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

        RequestContext AspNetDictionary.IPropertySource.GetRequestContext()
        {
            return _requestContext;
        }

        HttpContextBase AspNetDictionary.IPropertySource.GetHttpContextBase()
        {
            return _httpContext;
        }

        #endregion
    }
}
