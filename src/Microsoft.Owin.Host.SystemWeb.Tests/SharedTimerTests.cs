// -----------------------------------------------------------------------
// <copyright file="SharedTimerTests.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;

namespace Microsoft.Owin.Host.SystemWeb.Tests
{
    public class SharedTimerTests
    {
        [Fact]
        public void SharedTimer_Register_CallbackInvoked()
        {
            ManualResetEvent timerSet = new ManualResetEvent(false);
            using (SharedTimer timer = new SharedTimer(TimeSpan.FromMilliseconds(20)))
            {
                using (IDisposable cleanup = timer.Register(_ => timerSet.Set(), null))
                {
                    Assert.NotNull(cleanup);
                    Assert.True(timerSet.WaitOne(100));
                }
            }
        }

        [Fact]
        public void SharedTimer_Register_CallbackInvokedMultipleTimes()
        {
            ManualResetEvent timerSet = new ManualResetEvent(false);
            using (SharedTimer timer = new SharedTimer(TimeSpan.FromMilliseconds(20)))
            {
                using (IDisposable cleanup = timer.Register(_ => timerSet.Set(), null))
                {
                    Assert.NotNull(cleanup);
                    Assert.True(timerSet.WaitOne(100));
                    timerSet.Reset();
                    Assert.True(timerSet.WaitOne(100));
                    timerSet.Reset();
                }
                Assert.False(timerSet.WaitOne(100));
            }
        }

        [Fact]
        public void SharedTimer_RegisterState_CallbackInvokedWithState()
        {
            object myState = new object();
            bool? correctState = null;
            ManualResetEvent timerSet = new ManualResetEvent(false);
            using (SharedTimer timer = new SharedTimer(TimeSpan.FromMilliseconds(20)))
            {
                timer.Register(state =>
                {
                    correctState = (myState == state);
                    timerSet.Set();
                }, myState);
                Assert.True(timerSet.WaitOne(100));
                Assert.True(correctState.Value);
            }
        }

        [Fact]
        public void SharedTimer_Register5_CallbacksInvoked()
        {
            ManualResetEvent timerSet0 = new ManualResetEvent(false);
            ManualResetEvent timerSet1 = new ManualResetEvent(false);
            ManualResetEvent timerSet2 = new ManualResetEvent(false);
            ManualResetEvent timerSet3 = new ManualResetEvent(false);
            ManualResetEvent timerSet4 = new ManualResetEvent(false);
            using (SharedTimer timer = new SharedTimer(TimeSpan.FromMilliseconds(20)))
            {
                timer.Register(_ => timerSet0.Set(), null);
                timer.Register(_ => timerSet1.Set(), null);
                timer.Register(_ => timerSet2.Set(), null);
                timer.Register(_ => timerSet3.Set(), null);
                timer.Register(_ => timerSet4.Set(), null);
                Assert.True(timerSet0.WaitOne(100));
                Assert.True(timerSet1.WaitOne(100));
                Assert.True(timerSet2.WaitOne(100));
                Assert.True(timerSet3.WaitOne(100));
                Assert.True(timerSet4.WaitOne(100));
            }
        }

        [Fact]
        public void SharedTimer_DisposeRegistrationQuickly_NoCallback()
        {
            ManualResetEvent timerSet = new ManualResetEvent(false);
            using (SharedTimer timer = new SharedTimer(TimeSpan.FromMilliseconds(20)))
            {
                IDisposable cleanup = timer.Register(_ => timerSet.Set(), null);
                cleanup.Dispose();
                Assert.False(timerSet.WaitOne(100));
            }
        }

        [Fact]
        public void SharedTimer_Disposed_NoCallback()
        {
            ManualResetEvent timerSet = new ManualResetEvent(false);
            using (SharedTimer timer = new SharedTimer(TimeSpan.FromMilliseconds(20)))
            {
                timer.Register(_ => timerSet.Set(), null);
            }
            Assert.False(timerSet.WaitOne(100));
        }

        [Fact]
        public void SharedTimer_OneCallbackThrows_OtherCallbacksInvoked()
        {
            ManualResetEvent timerSet0 = new ManualResetEvent(false);
            ManualResetEvent timerSet1 = new ManualResetEvent(false);
            using (SharedTimer timer = new SharedTimer(TimeSpan.FromMilliseconds(20)))
            {
                timer.Register(_ => timerSet0.Set(), null);
                timer.Register(_ => { throw new InvalidOperationException(); }, null);
                timer.Register(_ => timerSet1.Set(), null);
                Assert.True(timerSet0.WaitOne(100));
                Assert.True(timerSet1.WaitOne(100));
            }
        }

        [Fact]
        public void SharedTimer_DisposeOne_OtherCallbacksInvoked()
        {
            ManualResetEvent timerSet0 = new ManualResetEvent(false);
            ManualResetEvent timerSet1 = new ManualResetEvent(false);
            ManualResetEvent timerSet2 = new ManualResetEvent(false);
            using (SharedTimer timer = new SharedTimer(TimeSpan.FromMilliseconds(20)))
            {
                IDisposable dispose0 = timer.Register(_ => timerSet0.Set(), null);
                IDisposable dispose1 = timer.Register(_ => timerSet1.Set(), null);
                IDisposable dispose2 = timer.Register(_ => timerSet2.Set(), null);
                dispose1.Dispose();
                Assert.True(timerSet0.WaitOne(100));
                Assert.True(timerSet2.WaitOne(100));
                Assert.False(timerSet1.WaitOne(100));
            }
        }
    }
}
