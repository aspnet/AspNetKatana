// <copyright file="OwinHttpListenerContext.cs" company="Katana contributors">
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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Microsoft.Owin.Host.HttpListener
{
    internal class OwinHttpListenerContext : IDisposable
    {
        private readonly HttpListenerContext _httpListenerContext;
        private readonly OwinHttpListenerRequest _owinRequest;
        private readonly OwinHttpListenerResponse _owinResponse;
        private readonly IDictionary<string, object> _environment;
        private readonly CancellationTokenSource _cts;

        private CancellationTokenRegistration _disconnectRegistration;

        internal OwinHttpListenerContext(HttpListenerContext httpListenerContext, string basePath)
        {
            _httpListenerContext = httpListenerContext;
            _cts = new CancellationTokenSource();
            _environment = new Dictionary<string, object>();
            _owinRequest = new OwinHttpListenerRequest(_httpListenerContext.Request, basePath, _environment);
            _owinResponse = new OwinHttpListenerResponse(_httpListenerContext, _environment);

            _environment.Add(Constants.VersionKey, Constants.OwinVersion);
            _environment.Add(Constants.CallCancelledKey, _cts.Token);

            _environment.Add(Constants.ServerUserKey, _httpListenerContext.User);
            _environment.Add(typeof(HttpListenerContext).FullName, _httpListenerContext);
        }

        internal IDictionary<string, object> Environment
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

            End();
        }

        private void End()
        {
            _owinResponse.End();
        }

        internal void RegisterForDisconnectNotice(CancellationToken ct)
        {
            _disconnectRegistration = ct.Register(SetDisconnected, this);
        }

        private static void SetDisconnected(object state)
        {
            OwinHttpListenerContext context = (OwinHttpListenerContext)state;
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
                _cts.Dispose();
                _disconnectRegistration.Dispose();
            }
        }
    }
}
