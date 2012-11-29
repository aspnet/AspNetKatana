// -----------------------------------------------------------------------
// <copyright file="ShutdownDetector.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal class ShutdownDetector : IRegisteredObject, IDisposable
    {
        private const string TraceName = "Microsoft.Owin.Host.SystemWeb.ShutdownDetector";

        private readonly CancellationTokenSource _cts;
        private readonly ITrace _trace;
        private IDisposable _checkAppPoolTimer;

        public ShutdownDetector()
        {
            _cts = new CancellationTokenSource();
            _trace = TraceFactory.Create(TraceName);
        }

        internal CancellationToken Token
        {
            get { return _cts.Token; }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Initialize must not throw")]
        internal void Initialize()
        {
            try
            {
                HostingEnvironment.RegisterObject(this);

                // Create a timer for detecting when the app pool has been requested for shutdown.
                // Normally when the appdomain shuts down IRegisteredObject.Stop gets called
                // but ASP.NET waits for requests to end before calling IRegisteredObject.Stop (This can be
                // troublesome for some frameworks like SignalR that keep long running requests alive).
                // This is a more aggressive check to see if the app domain is in the process of being shutdown and
                // we trigger the same cts in that case.
                if (HttpRuntime.UsingIntegratedPipeline && UnsafeIISMethods.CanDetectAppDomainRestart)
                {
#if !NET50
                    // Use the existing timer
                    _checkAppPoolTimer = SharedTimer.StaticTimer.Register(CheckForAppDomainRestart, state: null);
#else
                    _checkAppPoolTimer = new Timer(CheckForAppDomainRestart, state: null,
                        dueTime: TimeSpan.FromSeconds(10), period: TimeSpan.FromSeconds(10));
#endif
                }
            }
            catch (Exception ex)
            {
                _trace.WriteError(Resources.Exception_ShutdownDetectionSetup, ex);
            }
        }

        private void CheckForAppDomainRestart(object state)
        {
            if (UnsafeIISMethods.RequestedAppDomainRestart)
            {
                Cancel();
            }
        }

        public void Stop(bool immediate)
        {
            Cancel();
            HostingEnvironment.UnregisterObject(this);
        }

        private void Cancel()
        {
            // Stop the timer as we don't need it anymore
            if (_checkAppPoolTimer != null)
            {
                _checkAppPoolTimer.Dispose();
            }

            // Trigger the cancellation token
            try
            {
                _cts.Cancel(throwOnFirstException: false);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (AggregateException ag)
            {
                _trace.WriteError(Resources.Exception_OnShutdown, ag);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cts.Dispose();

                if (_checkAppPoolTimer != null)
                {
                    _checkAppPoolTimer.Dispose();
                }
            }
        }
    }
}
