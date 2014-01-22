// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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

                // Normally when the AppDomain shuts down IRegisteredObject.Stop gets called, except that
                // ASP.NET waits for requests to end before calling IRegisteredObject.Stop. This can be
                // troublesome for some frameworks like SignalR that keep long running requests alive.
                // These are more aggressive checks to see if the app domain is in the process of being shutdown and
                // we trigger the same cts in that case.
                if (HttpRuntime.UsingIntegratedPipeline)
                {
                    if (RegisterForStopListeningEvent())
                    {
                    }
                    else if (UnsafeIISMethods.CanDetectAppDomainRestart)
                    {
                        // Create a timer for polling when the app pool has been requested for shutdown.
                        _checkAppPoolTimer = new Timer(CheckForAppDomainRestart, state: null,
                            dueTime: TimeSpan.FromSeconds(10), period: TimeSpan.FromSeconds(10));
                    }
                }
            }
            catch (Exception ex)
            {
                _trace.WriteError(Resources.Trace_ShutdownDetectionSetupException, ex);
            }
        }

        // Note: When we have a compilation that targets .NET 4.5.1, implement IStopListeningRegisteredObject
        // instead of reflecting for HostingEnvironment.StopListening.
        private bool RegisterForStopListeningEvent()
        {
            EventInfo stopEvent = typeof(HostingEnvironment).GetEvent("StopListening");
            if (stopEvent == null)
            {
                return false;
            }
            stopEvent.AddEventHandler(null, new EventHandler(StopListening));
            return true;
        }

        private void StopListening(object sender, EventArgs e)
        {
            Cancel();
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
                _trace.WriteError(Resources.Trace_ShutdownException, ag);
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
