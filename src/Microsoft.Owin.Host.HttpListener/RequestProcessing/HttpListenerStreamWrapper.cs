// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;

namespace Microsoft.Owin.Host.HttpListener.RequestProcessing
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

        public override void Close()
        {
            // Disabled. The server will close the response when the AppFunc task completes.
        }

        [SuppressMessage("Microsoft.Usage", "CA2215:Dispose methods should call base class dispose", Justification = "By design")]
        protected override void Dispose(bool disposing)
        {
            // Disabled. The server will close the response when the AppFunc task completes.
        }
    }
}
