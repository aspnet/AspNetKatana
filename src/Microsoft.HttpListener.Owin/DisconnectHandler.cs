//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.HttpListener.Owin
{
    internal class DisconnectHandler
    {
        private readonly ConcurrentDictionary<ulong, Lazy<CancellationToken>> connectionCancellationTokens;
        private readonly System.Net.HttpListener listener;
        private CriticalHandle requestQueueHandle;
        private FieldInfo connectionIdField;

        /// <summary>
        /// Initializes a new instance of <see cref="DisconnectHandler"/>.
        /// </summary>
        /// <param name="listener">The <see cref="Server"/>'s HttpListener</param>
        internal DisconnectHandler(System.Net.HttpListener listener)
        {
            this.connectionCancellationTokens = new ConcurrentDictionary<ulong, Lazy<CancellationToken>>();
            this.listener = listener;
        }

        /// <summary>
        /// Initializes the Request Queue Handler.  Meant to be called once the servers <see cref="HttpListener"/> has been started.
        /// </summary>
        internal void Initialize()
        {
            // Get the request queue handle so we can register for disconnect
            var requestQueueHandleField = typeof(System.Net.HttpListener).GetField("m_RequestQueueHandle", BindingFlags.Instance | BindingFlags.NonPublic);

            // Get the connection id field info from the request object
            this.connectionIdField = typeof(HttpListenerRequest).GetField("m_ConnectionId", BindingFlags.Instance | BindingFlags.NonPublic);

            if (requestQueueHandleField != null)
            {
                this.requestQueueHandle = (CriticalHandle)requestQueueHandleField.GetValue(this.listener);
            }
        }

        /// <summary>
        /// Gets the <see cref="CancellationToken"/> associated with the <paramref name="context"/>.  
        /// If the <see cref="CancellationToken"/> does not exist for the given <paramref name="context"/> then <see cref="CreateToken"/> is called.
        /// </summary>
        /// <param name="context">The context for the current connection.</param>
        /// <returns>A cancellation token that is registered for disconnect for the current connection.</returns>
        internal CancellationToken GetDisconnectToken(HttpListenerContext context)
        {
            if (this.connectionIdField == null || this.requestQueueHandle == null)
            {
                Debug.WriteLine("Server: Unable to resolve requestQueue handle. Disconnect notifications will be ignored");
                return CancellationToken.None;
            }

            var connectionId = (ulong)this.connectionIdField.GetValue(context.Request);
            return this.connectionCancellationTokens.GetOrAdd(connectionId, key => new Lazy<CancellationToken>(() => this.CreateToken(key))).Value;
        }

        private unsafe CancellationToken CreateToken(ulong connectionId)
        {
            Debug.WriteLine("Server: Registering connection for disconnect for connection ID: " + connectionId);

            // Create a nativeOverlapped callback so we can register for disconnect callback
            var overlapped = new Overlapped();
            var cts = new CancellationTokenSource();

            var nativeOverlapped = overlapped.UnsafePack((errorCode, numBytes, pOVERLAP) =>
            {
                Debug.WriteLine("Server: http.sys disconnect callback fired for connection ID: " + connectionId);

                // Free the overlapped
                Overlapped.Free(pOVERLAP);

                // Pull the token out of the list and Cancel it.
                Lazy<CancellationToken> token;
                connectionCancellationTokens.TryRemove(connectionId, out token);
                try
                {
                    cts.Cancel();
                }
                catch (AggregateException)
                {
                }

                cts.Dispose();
            },
            null);

            uint hr = NativeMethods.HttpWaitForDisconnect(this.requestQueueHandle, connectionId, nativeOverlapped);

            if (hr != NativeMethods.HttpErrors.ERROR_IO_PENDING &&
                hr != NativeMethods.HttpErrors.NO_ERROR)
            {
                // We got an unknown result so return a None
                Debug.WriteLine("Unable to register disconnect callback: " + hr);
                return CancellationToken.None;
            }

            return cts.Token;
        }
    }
}