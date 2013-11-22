// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace Microsoft.Owin.Host.HttpListener
{
    [SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        internal enum HTTP_SERVER_PROPERTY
        {
            HttpServerAuthenticationProperty,
            HttpServerLoggingProperty,
            HttpServerQosProperty,
            HttpServerTimeoutsProperty,
            HttpServerQueueLengthProperty,
            HttpServerStateProperty,
            HttpServer503VerbosityProperty,
            HttpServerBindingProperty,
            HttpServerExtendedAuthenticationProperty,
            HttpServerListenEndpointProperty,
            HttpServerChannelBindProperty,
            HttpServerProtectionLevelProperty,
        }

        [DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true, ExactSpelling = true)]
        internal static extern uint HttpSetRequestQueueProperty(
            CriticalHandle requestQueueHandle,
            HTTP_SERVER_PROPERTY serverProperty,
            IntPtr pPropertyInfo,
            uint propertyInfoLength,
            uint reserved,
            IntPtr pReserved);

        internal static unsafe void SetRequestQueueLength(System.Net.HttpListener listener, long length)
        {
            Type listenerType = typeof(System.Net.HttpListener);
            PropertyInfo requestQueueHandleProperty = listenerType.GetProperty("RequestQueueHandle", BindingFlags.NonPublic | BindingFlags.Instance);

            CriticalHandle requestQueueHandle = (CriticalHandle)requestQueueHandleProperty.GetValue(listener, null);
            uint result = HttpSetRequestQueueProperty(requestQueueHandle, HTTP_SERVER_PROPERTY.HttpServerQueueLengthProperty,
                new IntPtr((void*)&length), (uint)Marshal.SizeOf(length), 0, IntPtr.Zero);

            if (result != 0)
            {
                throw new Win32Exception((int)result);
            }
        }

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
