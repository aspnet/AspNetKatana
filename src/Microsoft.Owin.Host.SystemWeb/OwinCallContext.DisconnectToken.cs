// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET50

using System;
using System.Threading;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal partial class OwinCallContext
    {
        private static readonly TimerCallback ConnectionTimerCallback = CheckIsClientConnected;

        private CancellationTokenSource _callCancelledSource;
        private IDisposable _connectionCheckTimer;

        internal CancellationToken BindDisconnectNotification()
        {
            _callCancelledSource = new CancellationTokenSource();
            _connectionCheckTimer = new Timer(ConnectionTimerCallback, state: this,
                dueTime: TimeSpan.FromSeconds(10), period: TimeSpan.FromSeconds(10));
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
