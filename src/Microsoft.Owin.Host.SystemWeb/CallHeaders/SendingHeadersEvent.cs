// <copyright file="SendingHeadersEvent.cs" company="Katana contributors">
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
using System.Threading;

namespace Microsoft.Owin.Host.SystemWeb.CallHeaders
{
    internal class SendingHeadersEvent
    {
        private IList<Tuple<Action<object>, object>> _callbacks = new List<Tuple<Action<object>, object>>();

        internal void Register(Action<object> callback, object state)
        {
            if (_callbacks == null)
            {
                throw new InvalidOperationException("Cannot register for event after headers are sent");
            }
            _callbacks.Add(new Tuple<Action<object>, object>(callback, state));
        }

        internal void Fire()
        {
            var callbacks = Interlocked.Exchange(ref _callbacks, null);
            var count = callbacks.Count;
            for (var index = 0; index != count; ++index)
            {
                var tuple = callbacks[count - index - 1];
                tuple.Item1(tuple.Item2);
            }
        }
    }
}
