// <copyright file="OwinWebSocketParameters.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Owin.Types.WebSockets
{
#region OwinWebSocketParameters

    internal partial struct OwinWebSocketParameters
    {
        public static OwinWebSocketParameters Create()
        {
            return new OwinWebSocketParameters(new ConcurrentDictionary<string, object>(StringComparer.Ordinal));
        }

        public static OwinWebSocketParameters Create(string subProtocol)
        {
            return new OwinWebSocketParameters(new ConcurrentDictionary<string, object>(StringComparer.Ordinal))
            {
                SubProtocol = subProtocol
            };
        }
    }
#endregion

#region OwinWebSocketParameters.Generated

    [System.CodeDom.Compiler.GeneratedCode("App_Packages", "")]
    internal partial struct OwinWebSocketParameters
    {
        private readonly IDictionary<string, object> _dictionary;

        public OwinWebSocketParameters(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        public IDictionary<string, object> Dictionary
        {
            get { return _dictionary; }
        }

#region Value-type equality
        public bool Equals(OwinWebSocketParameters other)
        {
            return Equals(_dictionary, other._dictionary);
        }

        public override bool Equals(object obj)
        {
            return obj is OwinWebSocketParameters && Equals((OwinWebSocketParameters)obj);
        }

        public override int GetHashCode()
        {
            return (_dictionary != null ? _dictionary.GetHashCode() : 0);
        }

        public static bool operator ==(OwinWebSocketParameters left, OwinWebSocketParameters right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OwinWebSocketParameters left, OwinWebSocketParameters right)
        {
            return !left.Equals(right);
        }
#endregion

        public T Get<T>(string key)
        {
            object value;
            return _dictionary.TryGetValue(key, out value) ? (T)value : default(T);
        }

        public OwinWebSocketParameters Set(string key, object value)
        {
            _dictionary[key] = value;
            return this;
        }

    }
#endregion

#region OwinWebSocketParameters.Spec-WebSocket

    internal partial struct OwinWebSocketParameters
    {
        public string SubProtocol
        {
            get { return Get<string>(OwinConstants.WebSocket.SubProtocol); }
            set { Set(OwinConstants.WebSocket.SubProtocol, value); }
        }
    }
#endregion

}
