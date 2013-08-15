// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
                throw new InvalidOperationException(Resources.Exception_CannotRegisterAfterHeadersSent);
            }
            _callbacks.Add(new Tuple<Action<object>, object>(callback, state));
        }

        internal void Fire()
        {
            IList<Tuple<Action<object>, object>> callbacks = Interlocked.Exchange(ref _callbacks, null);
            int count = callbacks.Count;
            for (int index = 0; index != count; ++index)
            {
                Tuple<Action<object>, object> tuple = callbacks[count - index - 1];
                tuple.Item1(tuple.Item2);
            }
        }
    }
}
