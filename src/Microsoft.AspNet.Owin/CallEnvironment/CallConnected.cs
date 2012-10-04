//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace Microsoft.AspNet.Owin.CallEnvironment
{
    /// <summary>
    /// Object for use in server.CallConnected key
    /// </summary>
    public class CallConnected
    {
        private readonly CallConnectedSource _source;

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
