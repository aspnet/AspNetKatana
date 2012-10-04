//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.AspNet.Owin.CallHeaders
{
    public class SendingHeadersEvent
    {
        private IList<Tuple<Action<object>, object>> _callbacks = new List<Tuple<Action<object>, object>>();

        public void Register(Action<object> callback, object state)
        {
            if (_callbacks == null)
            {
                throw new InvalidOperationException("Cannot register for event after headers are sent");
            }
            _callbacks.Add(new Tuple<Action<object>, object>(callback, state));
        }

        public void Fire()
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
