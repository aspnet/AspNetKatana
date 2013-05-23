// <copyright file="DisconnectHandler.cs" company="Microsoft Open Technologies, Inc.">
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

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.Owin.Host.HttpListener
{
    using LoggerFunc = Func<TraceEventType, int, object, Exception, Func<object, Exception, string>, bool>;

    internal class DisconnectHandler
    {
        private readonly ConcurrentDictionary<ulong, Lazy<CancellationToken>> _connectionCancellationTokens;
        private readonly System.Net.HttpListener _listener;
        private readonly CriticalHandle _requestQueueHandle;
        private readonly FieldInfo _connectionIdField;
        private readonly LoggerFunc _logger;

        internal DisconnectHandler(System.Net.HttpListener listener, LoggerFunc logger)
        {
            _connectionCancellationTokens = new ConcurrentDictionary<ulong, Lazy<CancellationToken>>();
            _listener = listener;
            _logger = logger;

            // Get the request queue handle so we can register for disconnect
            FieldInfo requestQueueHandleField = typeof(System.Net.HttpListener).GetField("m_RequestQueueHandle", BindingFlags.Instance | BindingFlags.NonPublic);

            // Get the connection id field info from the request object
            _connectionIdField = typeof(HttpListenerRequest).GetField("m_ConnectionId", BindingFlags.Instance | BindingFlags.NonPublic);

            if (requestQueueHandleField != null && requestQueueHandleField.FieldType == typeof(CriticalHandle))
            {
                _requestQueueHandle = (CriticalHandle)requestQueueHandleField.GetValue(_listener);
            }
            if (_connectionIdField == null || _requestQueueHandle == null)
            {
                LogHelper.LogInfo(_logger, Resources.Log_UnableToSetup);
            }
        }

        internal CancellationToken GetDisconnectToken(HttpListenerContext context)
        {
            if (_connectionIdField == null || _requestQueueHandle == null)
            {
                return CancellationToken.None;
            }

            var connectionId = (ulong)_connectionIdField.GetValue(context.Request);
            return _connectionCancellationTokens.GetOrAdd(connectionId, key => new Lazy<CancellationToken>(() => CreateToken(key))).Value;
        }

        private unsafe CancellationToken CreateToken(ulong connectionId)
        {
            // Create a nativeOverlapped callback so we can register for disconnect callback
            var overlapped = new Overlapped();
            var cts = new CancellationTokenSource();
            CancellationToken returnToken = cts.Token;

            NativeOverlapped* nativeOverlapped = overlapped.UnsafePack(
                (errorCode, numBytes, overlappedPtr) =>
                {
                    // Free the overlapped
                    Overlapped.Free(overlappedPtr);

                    if (errorCode != NativeMethods.HttpErrors.NO_ERROR)
                    {
                        LogHelper.LogException(_logger, "IOCompletionCallback", new Win32Exception((int)errorCode));
                    }

                    // Pull the token out of the list and Cancel it.
                    Lazy<CancellationToken> token;
                    _connectionCancellationTokens.TryRemove(connectionId, out token);

                    bool success = ThreadPool.UnsafeQueueUserWorkItem(CancelToken, cts);
                    Debug.Assert(success, "Unable to queue disconnect notification.");
                },
                null);

            uint hr = NativeMethods.HttpWaitForDisconnect(_requestQueueHandle, connectionId, nativeOverlapped);

            if (hr != NativeMethods.HttpErrors.ERROR_IO_PENDING &&
                hr != NativeMethods.HttpErrors.NO_ERROR)
            {
                // We got an unknown result, assume the connection has been closed.
                Overlapped.Free(nativeOverlapped);
                Lazy<CancellationToken> lazyToken;
                _connectionCancellationTokens.TryRemove(connectionId, out lazyToken);
                LogHelper.LogException(_logger, "HttpWaitForDisconnect", new Win32Exception((int)hr));
                cts.Cancel();
                cts.Dispose();
            }

            return returnToken;
        }

        private void CancelToken(object state)
        {
            CancellationTokenSource cts = (CancellationTokenSource)state;
            try
            {
                cts.Cancel();
            }
            catch (AggregateException age)
            {
                LogHelper.LogException(_logger, Resources.Log_AppDisonnectErrors, age);
            }
            cts.Dispose();
        }
    }
}
