//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Net;

namespace Microsoft.HttpListener.Owin
{
    internal class HttpListenerStreamWrapper : ExceptionFilterStream
    {
        internal HttpListenerStreamWrapper(Stream innerStream)
            : base(innerStream)
        {
        }

        // Convert HttpListenerExceptions to IOExceptions
        protected override bool TryWrapException(Exception ex, out Exception wrapped)
        {
            if (ex is HttpListenerException)
            {
                wrapped = new IOException(string.Empty, ex);
                return true;
            }

            wrapped = null;
            return false;
        }
    }
}