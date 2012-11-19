// <copyright file="OwinCallContext.DisconnectToken.cs" company="Katana contributors">
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

#if !NET50

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal partial class OwinCallContext 
    {
        private static readonly Action<object> ConnectionTimerCallback = CheckIsClientConnected;

        private CancellationTokenSource _callCancelledSource;
        private IDisposable _connectionCheckTimer;

        internal CancellationToken BindDisconnectNotification()
        {
            _callCancelledSource = new CancellationTokenSource();
            _connectionCheckTimer = SharedTimer.StaticTimer.Register(ConnectionTimerCallback, this);
            return _callCancelledSource.Token;
        }

        private void UnbindDisconnectNotification()
        {
            if (_callCancelledSource != null)
            {
                _callCancelledSource.Dispose();                
            }
            if (_connectionCheckTimer != null)
            {
                _connectionCheckTimer.Dispose();
            }
        }

        private static void CheckIsClientConnected(object obj)
        {
            OwinCallContext context = (OwinCallContext)obj;
            if (!context._httpResponse.IsClientConnected)
            {
                context._connectionCheckTimer.Dispose();
                SetDisconnected(context);
            }
        }

        private static void SetDisconnected(object obj)
        {
            OwinCallContext context = (OwinCallContext)obj;
            CancellationTokenSource cts = context._callCancelledSource;
            try
            {
                cts.Cancel(throwOnFirstException: false);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (AggregateException ag)
            {
                context._trace.WriteError(Resources.Exception_RequestDisconnect, ag);
            }
        }

        private void OnFaulted()
        {
            // called when write or flush encounters HttpException
            // on NET40 this causes cancel token to be signalled
            SetDisconnected(this);
        }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
