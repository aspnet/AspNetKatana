// <copyright file="OwinWebSocket.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2013 Microsoft Open Technologies, Inc. All rights reserved.
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CloseAsyncDelegate = System.Func<int, string, System.Threading.CancellationToken, System.Threading.Tasks.Task>;
using ReceiveAsyncDelegate = System.Func<System.ArraySegment<byte>, System.Threading.CancellationToken, System.Threading.Tasks.Task<System.Tuple<int, bool, int>>>;
using SendAsyncDelegate = System.Func<System.ArraySegment<byte>, int, bool, System.Threading.CancellationToken, System.Threading.Tasks.Task>;

namespace Owin.Types
{
#region OwinWebSocket
    
    internal partial struct OwinWebSocket
    {
        public Task SendAsync(ArraySegment<byte> data, int messageType, bool endOfMessage, CancellationToken cancel)
        {
            return SendAsyncDelegate.Invoke(data, messageType, endOfMessage, cancel);
        }

        public Task<OwinWebSocketReceiveMessage> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancel)
        {
            //TODO: avoid ContinueWith when completed synchronously
            return ReceiveAsyncDelegate.Invoke(buffer, cancel)
                .ContinueWith(tuple => new OwinWebSocketReceiveMessage(tuple.Result));
        }

        public Task CloseAsync(int closeStatus, string closeDescription, CancellationToken cancel)
        {
            return CloseAsyncDelegate.Invoke(closeStatus, closeDescription, cancel);
        }

        public Task CloseAsync(int closeStatus, CancellationToken cancel)
        {
            return CloseAsyncDelegate.Invoke(closeStatus, null, cancel);
        }

        public Task CloseAsync(CancellationToken cancel)
        {
            return CloseAsyncDelegate.Invoke(0, null, cancel);
        }
    }
#endregion

#region OwinWebSocket.Generated

    [System.CodeDom.Compiler.GeneratedCode("App_Packages", "")]
    internal partial struct OwinWebSocket
    {
        private readonly IDictionary<string, object> _dictionary;

        public OwinWebSocket(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        public IDictionary<string, object> Dictionary
        {
            get { return _dictionary; }
        }

#region Value-type equality
        public bool Equals(OwinWebSocket other)
        {
            return Equals(_dictionary, other._dictionary);
        }

        public override bool Equals(object obj)
        {
            return obj is OwinWebSocket && Equals((OwinWebSocket)obj);
        }

        public override int GetHashCode()
        {
            return (_dictionary != null ? _dictionary.GetHashCode() : 0);
        }

        public static bool operator ==(OwinWebSocket left, OwinWebSocket right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OwinWebSocket left, OwinWebSocket right)
        {
            return !left.Equals(right);
        }
#endregion

        public T Get<T>(string key)
        {
            object value;
            return _dictionary.TryGetValue(key, out value) ? (T)value : default(T);
        }

        public OwinWebSocket Set(string key, object value)
        {
            _dictionary[key] = value;
            return this;
        }

    }
#endregion

#region OwinWebSocket.Spec-WebSocket

    internal partial struct OwinWebSocket
    {
        public SendAsyncDelegate SendAsyncDelegate
        {
            get { return Get<SendAsyncDelegate>(OwinConstants.WebSocket.SendAsync); }
            set { Set(OwinConstants.WebSocket.SendAsync, value); }
        }

        public ReceiveAsyncDelegate ReceiveAsyncDelegate
        {
            get { return Get<ReceiveAsyncDelegate>(OwinConstants.WebSocket.ReceiveAsync); }
            set { Set(OwinConstants.WebSocket.ReceiveAsync, value); }
        }

        public CloseAsyncDelegate CloseAsyncDelegate
        {
            get { return Get<CloseAsyncDelegate>(OwinConstants.WebSocket.CloseAsync); }
            set { Set(OwinConstants.WebSocket.CloseAsync, value); }
        }

        public string Version
        {
            get { return Get<string>(OwinConstants.WebSocket.Version); }
            set { Set(OwinConstants.WebSocket.Version, value); }
        }

        public CancellationToken CallCancelled
        {
            get { return Get<CancellationToken>(OwinConstants.WebSocket.CallCancelled); }
            set { Set(OwinConstants.WebSocket.CallCancelled, value); }
        }

        public int ClientCloseStatus
        {
            get { return Get<int>(OwinConstants.WebSocket.ClientCloseStatus); }
            set { Set(OwinConstants.WebSocket.ClientCloseStatus, value); }
        }

        public string ClientCloseDescription
        {
            get { return Get<string>(OwinConstants.WebSocket.ClientCloseDescription); }
            set { Set(OwinConstants.WebSocket.ClientCloseDescription, value); }
        }
    }
#endregion

}
