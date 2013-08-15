// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Hosting.Utilities
{
    internal sealed class Disposable : MarshalByRefObject, IDisposable
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
