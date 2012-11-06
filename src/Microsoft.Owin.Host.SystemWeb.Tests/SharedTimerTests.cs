// <copyright file="SharedTimerTests.cs" company="Katana contributors">
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

#if NET40

using System;
using System.Threading;
using Xunit;

namespace Microsoft.Owin.Host.SystemWeb.Tests
{
    public class SharedTimerTests
    {
        [Fact]
        public void SharedTimer_Register_CallbackInvoked()
        {
            var timerSet = new ManualResetEvent(false);
            using (var timer = new SharedTimer(TimeSpan.FromMilliseconds(20)))
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
            var timerSet = new ManualResetEvent(false);
            using (var timer = new SharedTimer(TimeSpan.FromMilliseconds(20)))
            {
                using (IDisposable cleanup = timer.Register(_ => timerSet.Set(), null))
                {
                    Assert.NotNull(cleanup);
                    Assert.True(timerSet.WaitOne(500));
                    timerSet.Reset();
                    Assert.True(timerSet.WaitOne(500));
                    timerSet.Reset();
                }
                Assert.False(timerSet.WaitOne(500));
            }
        }

        [Fact]
        public void SharedTimer_RegisterState_CallbackInvokedWithState()
        {
            var myState = new object();
            bool? correctState = null;
            var timerSet = new ManualResetEvent(false);
            using (var timer = new SharedTimer(TimeSpan.FromMilliseconds(20)))
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
            var timerSet0 = new ManualResetEvent(false);
            var timerSet1 = new ManualResetEvent(false);
            var timerSet2 = new ManualResetEvent(false);
            var timerSet3 = new ManualResetEvent(false);
            var timerSet4 = new ManualResetEvent(false);
            using (var timer = new SharedTimer(TimeSpan.FromMilliseconds(20)))
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
            var timerSet = new ManualResetEvent(false);
            using (var timer = new SharedTimer(TimeSpan.FromMilliseconds(20)))
            {
                IDisposable cleanup = timer.Register(_ => timerSet.Set(), null);
                cleanup.Dispose();
                Assert.False(timerSet.WaitOne(100));
            }
        }

        [Fact]
        public void SharedTimer_Disposed_NoCallback()
        {
            var timerSet = new ManualResetEvent(false);
            using (var timer = new SharedTimer(TimeSpan.FromMilliseconds(20)))
            {
                timer.Register(_ => timerSet.Set(), null);
            }
            Assert.False(timerSet.WaitOne(100));
        }

        [Fact]
        public void SharedTimer_OneCallbackThrows_OtherCallbacksInvoked()
        {
            var timerSet0 = new ManualResetEvent(false);
            var timerSet1 = new ManualResetEvent(false);
            using (var timer = new SharedTimer(TimeSpan.FromMilliseconds(20)))
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
            var timerSet0 = new ManualResetEvent(false);
            var timerSet1 = new ManualResetEvent(false);
            var timerSet2 = new ManualResetEvent(false);
            using (var timer = new SharedTimer(TimeSpan.FromMilliseconds(20)))
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

#endif