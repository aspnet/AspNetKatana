using System;

namespace Katana.Engine.Utils
{
    public class Disposable : MarshalByRefObject, IDisposable
    {
        private readonly Action _dispose;

        public Disposable(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            _dispose.Invoke();
        }
    }
}