// <copyright file="OwinCallContext.DisconnectToken.cs" company="Microsoft Open Technologies, Inc.">
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

#if !NET50

using System;
using System.Threading;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal partial class OwinCallContext
    {
#if NET40
        private static readonly Action<object> ConnectionTimerCallback = CheckIsClientConnected;
#else
        private static readonly TimerCallback ConnectionTimerCallback = CheckIsClientConnected;
#endif

        private CancellationTokenSource _callCancelledSource;
        private IDisposable _connectionCheckTimer;

        internal CancellationToken BindDisconnectNotification()
        {
            _callCancelledSource = new CancellationTokenSource();
#if NET40
            _connectionCheckTimer = SharedTimer.StaticTimer.Register(ConnectionTimerCallback, this);
#else
            _connectionCheckTimer = new Timer(ConnectionTimerCallback, state: this,
                dueTime: TimeSpan.FromSeconds(10), period: TimeSpan.FromSeconds(10));
#endif
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
            var context = (OwinCallContext)obj;
            if (!context._httpResponse.IsClientConnected)
            {
                context._connectionCheckTimer.Dispose();
                SetDisconnected(context);
            }
        }

        private static void SetDisconnected(object obj)
        {
            var context = (OwinCallContext)obj;
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
                Trace.WriteError(Resources.Trace_RequestDisconnectCallbackExceptions, ag);
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
