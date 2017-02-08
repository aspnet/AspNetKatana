// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading;
using System.Web;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal class DisconnectWatcher : IDisposable
    {
        private const string TraceName = "Microsoft.Owin.Host.SystemWeb.DisconnectWatcher";
        private static readonly ITrace Trace = TraceFactory.Create(TraceName);
        private static readonly TimerCallback ConnectionTimerCallback = CheckIsClientConnected;
        private static readonly bool IsSystemWebVersion451OrGreater = CheckIsSystemWebVersion451OrGreater();
        private static readonly bool IsClientDisconnectedTokenAvailable = CheckIsClientDisconnectedTokenAvailable();

        private readonly HttpResponseBase _httpResponse;

        private CancellationTokenSource _callCancelledSource;
        private IDisposable _connectionCheckTimer;

        internal DisconnectWatcher(HttpResponseBase httpResponse)
        {
            _httpResponse = httpResponse;
        }

        internal CancellationToken BindDisconnectNotification()
        {
            if (IsClientDisconnectedTokenAvailable && IsSystemWebVersion451OrGreater)
            {
                return _httpResponse.ClientDisconnectedToken;
            }

            _callCancelledSource = new CancellationTokenSource();
            _connectionCheckTimer = new Timer(ConnectionTimerCallback, state: this,
                dueTime: TimeSpan.FromSeconds(10), period: TimeSpan.FromSeconds(10));
            return _callCancelledSource.Token;
        }

        public void Dispose()
        {
            UnbindDisconnectNotification();
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
            var context = (DisconnectWatcher)obj;
            if (!context._httpResponse.IsClientConnected)
            {
                context._connectionCheckTimer.Dispose();
                SetDisconnected(context);
            }
        }

        private static void SetDisconnected(object obj)
        {
            var context = (DisconnectWatcher)obj;
            CancellationTokenSource cts = context._callCancelledSource;
            if (cts == null)
            {
                return;
            }
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

        internal void OnFaulted()
        {
            // called when write or flush encounters HttpException
            // on pre-Net4.5.1 this causes cancel token to be signaled
            SetDisconnected(this);
        }

        private static bool CheckIsClientDisconnectedTokenAvailable()
        {
            // Accessing HttpResponse.ClientDisconnectedToken throws PlatformNotSupportedException unless both:
            // 1) Using IIS 7.5 or newer, and
            // 2) Using integrated pipeline
            Version iis75 = new Version(7, 5);
            Version iisVersion = HttpRuntime.IISVersion;
            return iisVersion != null && iisVersion >= iis75 && HttpRuntime.UsingIntegratedPipeline;
        }

        private static bool CheckIsSystemWebVersion451OrGreater()
        {
            Assembly systemWeb = typeof(HttpContextBase).Assembly;
            // System.Web.AspNetEventSource only exists in .NET 4.5.1 and will not be back-ported to .NET 4.5.
            return systemWeb.GetType("System.Web.AspNetEventSource") != null;
        }
    }
}
