// <copyright file="NativeMethods.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.Owin.Host.HttpListener
{
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
