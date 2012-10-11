// <copyright file="CallConnectedSource.cs" company="Katana contributors">
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
