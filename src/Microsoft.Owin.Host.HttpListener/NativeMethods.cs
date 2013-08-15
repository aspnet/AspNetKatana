// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace Microsoft.Owin.Host.HttpListener
{
    [SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        [DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true, ExactSpelling = true)]
        internal static extern unsafe uint HttpWaitForDisconnect(CriticalHandle requestQueueHandle, ulong connectionId, NativeOverlapped* pOverlapped);

        internal static class HttpErrors
        {
            // ReSharper disable InconsistentNaming
            public const int NO_ERROR = 0x0;
            public const int ERROR_IO_PENDING = 0x3E5;
            // ReSharper restore InconsistentNaming
        }
    }
}
