// <copyright file="SharedTimer.cs" company="Katana contributors">
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

// Only use this type if we're not using ClientDisconnectToken
#if !NET50

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;

namespace Microsoft.Owin.Host.SystemWeb
{
    // Uses a single timer to track events for a large number of objects.
    internal class SharedTimer : IDisposable
    {
        private const string TraceName = "Microsoft.Owin.Host.SystemWeb.SharedTimer";

        private static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(5);
        private static readonly SharedTimer GlobalTimer = new SharedTimer(DefaultInterval);

        private readonly LinkedList<TimerRegistration> _registrations;
        private readonly TimeSpan _interval;
        private readonly Timer _timer;
        private readonly object _processLock;
        private readonly object _addLock;
        private readonly ITrace _trace;

        private LinkedList<TimerRegistration> _newRegistrations;
        private LinkedList<TimerRegistration> _emptyList;

        internal SharedTimer(TimeSpan interval)
        {
            Contract.Assert(interval > TimeSpan.Zero);
            _trace = TraceFactory.Create(TraceName);

            _interval = interval;
            _processLock = new object();
            _addLock = new object();
            _registrations = new LinkedList<TimerRegistration>();
            _newRegistrations = new LinkedList<TimerRegistration>();
            _emptyList = new LinkedList<TimerRegistration>();
            _timer = new Timer(ProcessTimers, null, _interval, _interval);
        }

        internal static SharedTimer StaticTimer
        {
            get { return GlobalTimer; }
        }

        private void ProcessTimers(object ignored)
        {
            // Lock to prevent the timer from firing multiple times (primarily for debug)
            lock (_processLock)
            {
                // Purge old timers
                Purge(_registrations);
                // Invoke callbacks
                InvokeCallbacks(_registrations);
                // Swap empty and new timers lists
                LinkedList<TimerRegistration> newTimers;
                lock (_addLock)
                {
                    newTimers = _newRegistrations;
                    _newRegistrations = _emptyList;
                }
                // Purge new timers, they may have already be canceled
                Purge(newTimers);
                // Move new timers to old timers
                Append(_registrations, newTimers);
                _emptyList = newTimers;
            }
        }

        private static void Purge(LinkedList<TimerRegistration> registrations)
        {
            LinkedListNode<TimerRegistration> nextNode;
            LinkedListNode<TimerRegistration> currentNode = registrations.First;
            while (currentNode != null)
            {
                nextNode = currentNode.Next;
                TimerRegistration registration = currentNode.Value;
                if (registration.Disposed)
                {
                    registrations.Remove(currentNode);
                }
                currentNode = nextNode;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Prevent exceptions from crashing the timer")]
        private void InvokeCallbacks(LinkedList<TimerRegistration> registrations)
        {
            LinkedListNode<TimerRegistration> nextNode = registrations.First;
            while (nextNode != null)
            {
                TimerRegistration registration = nextNode.Value;
                try
                {
                    registration.InvokeCallback();
                }
                catch (Exception ex)
                {
                    _trace.WriteError(Resources.Trace_TimerCallbackException, ex);
                }
                nextNode = nextNode.Next;
            }
        }

        // Remove all items from list two and add them to list one.
        private static void Append(LinkedList<TimerRegistration> listOne, LinkedList<TimerRegistration> listTwo)
        {
            LinkedListNode<TimerRegistration> nextNode;
            LinkedListNode<TimerRegistration> currentNode = listTwo.First;
            while (currentNode != null)
            {
                nextNode = currentNode.Next;
                listTwo.Remove(currentNode);
                listOne.AddLast(currentNode);
                currentNode = nextNode;
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller")]
        internal IDisposable Register(Action<object> callback, object state)
        {
            Contract.Assert(callback != null);
            TimerRegistration registration = new TimerRegistration(callback, state);
            lock (_addLock)
            {
                _newRegistrations.AddLast(registration);
            }
            return registration;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
            }
        }

        private class TimerRegistration : IDisposable
        {
            internal TimerRegistration(Action<object> callback, object state)
            {
                Callback = callback;
                State = state;
            }

            internal bool Disposed { get; private set; }
            private Action<object> Callback { get; set; }
            private object State { get; set; }

            internal void InvokeCallback()
            {
                // Prevent invoking the callback during disposal, or disposing during the callback.
                // See Dispose(true).
                lock (this)
                {
                    if (!Disposed)
                    {
                        Callback(State);
                    }
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
                    // Prevent invoking the callback during disposal, or disposing during the callback.
                    // This method's contents aren't dangerous, but the cleanup code the application will run
                    // afterwards may make invoking the callback dangerous.
                    lock (this)
                    {
                        Disposed = true;
                    }
                }
            }
        }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
