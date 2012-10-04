//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace Katana.Engine.Utils
{
    public class Disposable : MarshalByRefObject, IDisposable
    {
        private readonly Action _dispose;

        public Disposable(Action dispose)
        {
            this._dispose = dispose;
        }

        public void Dispose()
        {
            this._dispose.Invoke();
        }
    }
}