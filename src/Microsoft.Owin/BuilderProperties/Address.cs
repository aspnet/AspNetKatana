// <copyright file="Address.cs" company="Microsoft Open Technologies, Inc.">
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
    /// Wraps an address in the host.Addresses list.
    /// </summary>
    public struct Address
    {
        private readonly IDictionary<string, object> _dictionary;

        /// <summary>
        /// Create a new Address wrapper
        /// </summary>
        /// <param name="dictionary"></param>
        public Address(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        /// <summary>
        /// Create a new Address from the given parts
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="path"></param>
        public Address(string scheme, string host, string port, string path)
            : this(new Dictionary<string, object>())
        {
            Scheme = scheme;
            Host = host;
            Port = port;
            Path = path;
        }

        /// <summary>
        /// Access the underlying IDictionary
        /// </summary>
        public IDictionary<string, object> Dictionary
        {
            get { return _dictionary; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Scheme
        {
            get { return Get<string>(OwinConstants.CommonKeys.Scheme); }
            set { Set(OwinConstants.CommonKeys.Scheme, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Host
        {
            get { return Get<string>(OwinConstants.CommonKeys.Host); }
            set { Set(OwinConstants.CommonKeys.Host, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Port
        {
            get { return Get<string>(OwinConstants.CommonKeys.Port); }
            set { Set(OwinConstants.CommonKeys.Port, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Path
        {
            get { return Get<string>(OwinConstants.CommonKeys.Path); }
            set { Set(OwinConstants.CommonKeys.Path, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Address Create()
        {
            return new Address(new Dictionary<string, object>());
        }

#region Value-type equality
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Address other)
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
            return obj is Address && Equals((Address)obj);
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
        public static bool operator ==(Address left, Address right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(Address left, Address right)
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
        public Address Set(string key, object value)
        {
            _dictionary[key] = value;
            return this;
        }
    }
}
