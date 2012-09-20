using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.AspNet.Owin.CallHeaders
{
    public class SendingHeadersEvent
    {
        IList<Action> _callbacks = new List<Action>();

        public void Register(Action callback)
        {
            if (_callbacks == null)
            {
                throw new InvalidOperationException("Cannot register for event after headers are sent");
            }
            _callbacks.Add(callback);
        }

        public void Fire()
        {
            var callbacks = Interlocked.Exchange(ref _callbacks, null);
            var count = callbacks.Count;
            for (var index = 0; index != count; ++index)
            {
                callbacks[count - index - 1].Invoke();
            }
        }
    }
}
