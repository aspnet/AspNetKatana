// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        private readonly ConcurrentDictionary<ulong, ConnectionCancellation> _connectionCancellationTokens;
        private readonly System.Net.HttpListener _listener;
        private readonly CriticalHandle _requestQueueHandle;
        private readonly FieldInfo _connectionIdField;
        private readonly LoggerFunc _logger;

        internal DisconnectHandler(System.Net.HttpListener listener, LoggerFunc logger)
        {
            _connectionCancellationTokens = new ConcurrentDictionary<ulong, ConnectionCancellation>();
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
                LogHelper.LogInfo(_logger, "Unable to resolve handles. Disconnect notifications will be ignored.");
            }
        }

        internal CancellationToken GetDisconnectToken(HttpListenerContext context)
        {
            if (_connectionIdField == null || _requestQueueHandle == null)
            {
                return CancellationToken.None;
            }

            var connectionId = (ulong)_connectionIdField.GetValue(context.Request);
            ConnectionCancellation cancellation = GetConnectionCancellation(connectionId);
            return cancellation.GetCancellationToken(this, connectionId);
        }

        private ConnectionCancellation GetConnectionCancellation(ulong connectionId)
        {
            // Read case is performance senstive
            ConnectionCancellation cancellation;
            if (_connectionCancellationTokens.TryGetValue(connectionId, out cancellation))
            {
                return cancellation;
            }
            return GetCreatedConnectionCancellation(connectionId);
        }

        private ConnectionCancellation GetCreatedConnectionCancellation(ulong connectionId)
        {
            // Race condition on creation has no side effects
            var cancellation = new ConnectionCancellation();
            return _connectionCancellationTokens.GetOrAdd(connectionId, cancellation);
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
                    ConnectionCancellation cancellation;
                    _connectionCancellationTokens.TryRemove(connectionId, out cancellation);

                    bool success = ThreadPool.UnsafeQueueUserWorkItem(CancelToken, cts);
                    Debug.Assert(success, "Unable to queue disconnect notification.");
                },
                null);

            uint hr = NativeMethods.HttpWaitForDisconnectEx(_requestQueueHandle, connectionId, 0, nativeOverlapped);

            if (hr != NativeMethods.HttpErrors.ERROR_IO_PENDING &&
                hr != NativeMethods.HttpErrors.NO_ERROR)
            {
                // We got an unknown result, assume the connection has been closed.
                Overlapped.Free(nativeOverlapped);
                ConnectionCancellation cancellation;
                _connectionCancellationTokens.TryRemove(connectionId, out cancellation);
                LogHelper.LogException(_logger, "HttpWaitForDisconnectEx", new Win32Exception((int)hr));
                cts.Cancel();
                cts.Dispose();
            }

            return returnToken;
        }

        private void CancelToken(object state)
        {
            var cts = (CancellationTokenSource)state;
            try
            {
                cts.Cancel();
            }
            catch (AggregateException age)
            {
                LogHelper.LogException(_logger, "App errors on disconnect notification.", age);
            }
            cts.Dispose();
        }

        private class ConnectionCancellation
        {
            private volatile bool _initialized; // Must be volatile because initialization is synchronized
            private CancellationToken _cancellationToken;

            internal CancellationToken GetCancellationToken(DisconnectHandler disconnectHandler, ulong connectionId)
            {
                // Initialized case is performance sensitive
                if (_initialized)
                {
                    return _cancellationToken;
                }
                return InitializeCancellationToken(disconnectHandler, connectionId);
            }

            private CancellationToken InitializeCancellationToken(DisconnectHandler disconnectHandler, ulong connectionId)
            {
                object syncObject = this;
#pragma warning disable 420 // Disable warning about volatile by reference since EnsureInitialized does volatile operations
                return LazyInitializer.EnsureInitialized(ref _cancellationToken, ref _initialized, ref syncObject, () => disconnectHandler.CreateToken(connectionId));
#pragma warning restore 420
            }
        }
    }
}
