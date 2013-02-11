// <copyright file="OwinHttpListenerContext.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.Host.HttpListener.RequestProcessing
{
    using WebSocketAccept =
        Action<IDictionary<string, object>, // WebSocket Accept parameters
            Func<IDictionary<string, object>, // WebSocket environment
                Task /* Complete */>>;

    internal class OwinHttpListenerContext : IDisposable, CallEnvironment.IPropertySource
    {
        private readonly HttpListenerContext _httpListenerContext;
        private readonly OwinHttpListenerRequest _owinRequest;
        private readonly OwinHttpListenerResponse _owinResponse;
        private readonly CallEnvironment _environment;
        private readonly DisconnectHandler _disconnectHandler;

        private CancellationTokenSource _cts;
        private CancellationTokenRegistration _disconnectRegistration;

        internal OwinHttpListenerContext(HttpListenerContext httpListenerContext, string basePath, string path, string query, DisconnectHandler disconnectHandler)
        {
            _httpListenerContext = httpListenerContext;
            _environment = new CallEnvironment(this);
            _owinRequest = new OwinHttpListenerRequest(_httpListenerContext.Request, basePath, path, query, _environment);
            _owinResponse = new OwinHttpListenerResponse(_httpListenerContext, _environment);
            _disconnectHandler = disconnectHandler;

            _environment.OwinVersion = Constants.OwinVersion;

            _environment.ServerUser = _httpListenerContext.User;
            _environment.RequestContext = _httpListenerContext;
        }

        internal CallEnvironment Environment
        {
            get { return _environment; }
        }

        internal OwinHttpListenerRequest Request
        {
            get { return _owinRequest; }
        }

        internal OwinHttpListenerResponse Response
        {
            get { return _owinResponse; }
        }

        internal void End(Exception ex)
        {
            if (ex != null)
            {
                // TODO: LOG
                // Lazy initialized
                if (_cts != null)
                {
                    try
                    {
                        _cts.Cancel();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    catch (AggregateException)
                    {
                        // TODO: LOG
                    }
                }
            }

            End();
        }

        private void End()
        {
            try
            {
                _disconnectRegistration.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // CTR.Dispose() may throw an ODE on 4.0 if the CTS has previously been disposed.  Removed in 4.5.
            }
            _owinResponse.End();
        }

        private static void SetDisconnected(object state)
        {
            var context = (OwinHttpListenerContext)state;
            context.End(new HttpListenerException(Constants.ErrorConnectionNoLongerValid));
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    _disconnectRegistration.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // CTR.Dispose() may throw an ODE on 4.0 if the CTS has previously been disposed.  Removed in 4.5.
                }
                if (_cts != null)
                {
                    _cts.Dispose();
                }
            }
        }

        // Lazy environment initialization

        public CancellationToken GetCallCancelled()
        {
            _cts = new CancellationTokenSource();
            CancellationToken ct = _disconnectHandler.GetDisconnectToken(_httpListenerContext);
            _disconnectRegistration = ct.Register(SetDisconnected, this);
            return _cts.Token;
        }

        public Stream GetRequestBody()
        {
            return _owinRequest.GetRequestBody();
        }

        public string GetServerRemoteIpAddress()
        {
            return _owinRequest.GetRemoteIpAddress();
        }

        public string GetServerRemotePort()
        {
            return _owinRequest.GetRemotePort();
        }

        public string GetServerLocalIpAddress()
        {
            return _owinRequest.GetLocalIpAddress();
        }

        public string GetServerLocalPort()
        {
            return _owinRequest.GetLocalPort();
        }

        public bool GetServerIsLocal()
        {
            return _owinRequest.GetIsLocal();
        }

        public bool TryGetClientCert(ref X509Certificate value)
        {
            return _owinRequest.TryGetClientCert(ref value);
        }

        public bool TryGetWebSocketAccept(ref WebSocketAccept websocketAccept)
        {
            return _owinResponse.TryGetWebSocketAccept(ref websocketAccept);
        }
    }
}
