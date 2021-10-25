// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
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
        private IPrincipal _user;

        internal OwinHttpListenerContext(HttpListenerContext httpListenerContext, string basePath, string path, string query, DisconnectHandler disconnectHandler)
        {
            _httpListenerContext = httpListenerContext;
            _environment = new CallEnvironment(this);
            _owinRequest = new OwinHttpListenerRequest(_httpListenerContext.Request, basePath, path, query, _environment);
            _owinResponse = new OwinHttpListenerResponse(_httpListenerContext, _environment);
            _disconnectHandler = disconnectHandler;

            _environment.OwinVersion = Constants.OwinVersion;

            SetServerUser(_httpListenerContext.User);
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
                CancelDisconnectToken();
            }

            End();
        }

        internal void End()
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
            context.CancelDisconnectToken();
        }

        private void CancelDisconnectToken()
        {
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

        public IPrincipal GetServerUser()
        {
            return _user;
        }

        public void SetServerUser(IPrincipal user)
        {
            _user = user;
            Thread.CurrentPrincipal = _user;
        }

        public bool TryGetClientCert(ref X509Certificate value)
        {
            Exception clientCertErrors = null;
            bool result = _owinRequest.TryGetClientCert(ref value, ref clientCertErrors);
            Environment.ClientCertErrors = clientCertErrors;
            return result;
        }

        public bool TryGetClientCertErrors(ref Exception value)
        {
            X509Certificate clientCert = null;
            bool result = _owinRequest.TryGetClientCert(ref clientCert, ref value);
            Environment.ClientCert = clientCert;
            return result;
        }

        public bool TryGetWebSocketAccept(ref WebSocketAccept websocketAccept)
        {
            return _owinResponse.TryGetWebSocketAccept(ref websocketAccept);
        }
    }
}
