//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Owin.CallEnvironment
{
    public class CallConnectedSource
    {
        private readonly Action _update;
        private readonly object _completeLock = new object();
        private volatile bool _completeCalled;
        private volatile IList<Action> _completeContinuations;

        public CallConnectedSource(Action update)
        {
            _update = update;
            CallConnected = new CallConnected(this);
        }

        public CallConnected CallConnected { get; private set; }

        public bool IsConnected
        {
            get
            {
                if (!_completeCalled)
                {
                    _update.Invoke();
                }
                return _completeCalled;
            }
        }

        public void Register(Action continuation)
        {
            if (!_completeCalled)
            {
                lock (_completeLock)
                {
                    if (!_completeCalled)
                    {
                        if (_completeContinuations == null)
                        {
                            _completeContinuations = new List<Action>();
                        }
                        _completeContinuations.Add(continuation);
                        return;
                    }
                }
            }

            try
            {
                continuation.Invoke();
            }
            catch
            {
            }
        }

        public void Complete()
        {
            lock (_completeLock)
            {
                if (!_completeCalled)
                {
                    var continuations = _completeContinuations;
                    _completeContinuations = null;

                    if (continuations != null)
                    {
                        foreach (var continuation in continuations)
                        {
                            try
                            {
                                continuation.Invoke();
                            }
                            catch
                            {
                            }
                        }
                    }

                    _completeCalled = true;
                }
            }
        }
    }
}
