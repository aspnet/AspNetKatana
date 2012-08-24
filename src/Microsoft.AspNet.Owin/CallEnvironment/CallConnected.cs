using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Owin.CallEnvironment
{
    public class CallConnectedSource
    {
        readonly Action _update;
        readonly object _completeLock = new object();
        volatile bool _completeCalled;
        volatile IList<Action> _completeContinuations;

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

            try { continuation.Invoke(); }
            catch { }
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
                            try { continuation.Invoke(); }
                            catch { }
                        }
                    }

                    _completeCalled = true;
                }
            }
        }
    }

    /// <summary>
    /// Object for use in server.CallConnected key
    /// </summary>
    public class CallConnected
    {
        readonly CallConnectedSource _source;

        public CallConnected(CallConnectedSource source)
        {
            _source = source;
        }

        public bool IsConnected
        {
            get { return _source.IsConnected; }
        }

        public void Register(Action continuation)
        {
            _source.Register(continuation);
        }
    }
}
