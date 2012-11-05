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

namespace Microsoft.Owin.Host.SystemWeb
{
    internal class ShutdownDetector : IRegisteredObject, IDisposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private IDisposable _checkAppPoolTimer;
        
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
#if NET40
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
                Debug.WriteLine(ex.Message);
            }
        }

        private void CheckForAppDomainRestart(object state)
        {
            if (UnsafeIISMethods.RequestedAppDomainRestart)
            {
                // Stop the timer as we don't need it anymore
                _checkAppPoolTimer.Dispose();

                // Trigger the cancellation token
                try
                {
                    _cts.Cancel(throwOnFirstException: false);
                }
                catch (ObjectDisposedException)
                {
                }
                catch (AggregateException)
                {
                    // TODO: Trace
                }
            }
        }

        public void Stop(bool immediate)
        {
            try
            {
                _cts.Cancel(throwOnFirstException: false);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (AggregateException)
            {
                // Swallow the exception as Stop should never throw
                // TODO: Log exceptions
            }
            finally
            {
                HostingEnvironment.UnregisterObject(this);
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
