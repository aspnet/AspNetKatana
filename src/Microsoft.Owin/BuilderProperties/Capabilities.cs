// <copyright file="Capabilities.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Generic;

namespace Microsoft.Owin.BuilderProperties
{
    /// <summary>
    /// A wrapper for the server.Capabilities IDictionary
    /// </summary>
    public struct Capabilities
    {
        private readonly IDictionary<string, object> _dictionary;

        /// <summary>
        /// Create a new wrapper
        /// </summary>
        /// <param name="dictionary"></param>
        public Capabilities(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        /// <summary>
        /// The underling IDictionary
        /// </summary>
        public IDictionary<string, object> Dictionary
        {
            get { return _dictionary; }
        }

        /// <summary>
        /// sendfile.Version
        /// </summary>
        public string SendFileVersion
        {
            get { return Get<string>(OwinConstants.SendFiles.Version); }
            set { Set(OwinConstants.SendFiles.Version, value); }
        }

        // TODO: sendfile.Support IDictionary<string, object> containing sendfile.Concurrency. Only supported by HttpSys.
        /* TODO: Http.Sys only
        /// <summary>
        /// opaque.Version
        /// </summary>
        public string OpaqueVersion
        {
            get { return Get<string>(OwinConstants.OpaqueConstants.Version); }
            set { Set(OwinConstants.OpaqueConstants.Version, value); }
        }
        */

        /// <summary>
        /// websocket.Version
        /// </summary>
        public string WebSocketVersion
        {
            get { return Get<string>(OwinConstants.WebSocket.Version); }
            set { Set(OwinConstants.WebSocket.Version, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Capabilities Create()
        {
            return new Capabilities(new Dictionary<string, object>());
        }

#region Value-type equality

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Capabilities other)
        {
            return Equals(_dictionary, other._dictionary);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is Capabilities && Equals((Capabilities)obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (_dictionary != null ? _dictionary.GetHashCode() : 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(Capabilities left, Capabilities right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(Capabilities left, Capabilities right)
        {
            return !left.Equals(right);
        }

#endregion

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            object value;
            return _dictionary.TryGetValue(key, out value) ? (T)value : default(T);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Capabilities Set(string key, object value)
        {
            _dictionary[key] = value;
            return this;
        }
    }
}
