// -----------------------------------------------------------------------
// <copyright file="SharedTimer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#if NET40

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;

namespace Microsoft.Owin.Host.SystemWeb
{
    // Uses a single timer to track events for a large number of objects.
    public class SharedTimer : IDisposable
    {
        private static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(5);
        private static readonly SharedTimer GlobalTimer = new SharedTimer(DefaultInterval);

        private LinkedList<TimerRegistration> _registrations;
        private LinkedList<TimerRegistration> _newRegistrations;
        private LinkedList<TimerRegistration> _emptyList;
        private TimeSpan _interval;
        private Timer _timer;
        private object _processLock;
        private object _addLock;

        public SharedTimer(TimeSpan interval)
        {
            Contract.Assert(interval > TimeSpan.Zero);
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

        private static void InvokeCallbacks(LinkedList<TimerRegistration> registrations)
        {
            LinkedListNode<TimerRegistration> nextNode = registrations.First;
            while (nextNode != null)
            {
                TimerRegistration registration = nextNode.Value;
                if (!registration.Disposed)
                {
                    try
                    {
                        registration.Callback(registration.State);
                    }
                    catch (Exception)
                    {
                        // TODO: Log
                    }
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

        public IDisposable Register(Action<object> callback, object state)
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
            internal Action<object> Callback { get; private set; }
            internal object State { get; private set; }
            
            public void Dispose()
            {
                Dispose(true);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Disposed = true;
                }
            }
        }
    }
}

#endif
