// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Owin.BuilderProperties
{
    /// <summary>
    /// Contains the parts of an address.
    /// </summary>
    public struct Address
    {
        private readonly IDictionary<string, object> _dictionary;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="dictionary"></param>
        public Address(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        /// <summary>
        /// Initializes a new <see cref="T:Microsoft.Owin.BuilderProperties.Address"/> with the given parts.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="path">The path.</param>
        public Address(string scheme, string host, string port, string path)
            : this(new Dictionary<string, object>())
        {
            Scheme = scheme;
            Host = host;
            Port = port;
            Path = path;
        }

        /// <summary>
        /// Gets the internal dictionary for this collection.
        /// </summary>
        /// <returns>The internal dictionary for this collection.</returns>
        public IDictionary<string, object> Dictionary
        {
            get { return _dictionary; }
        }

        /// <summary>
        /// The uri scheme.
        /// </summary>
        public string Scheme
        {
            get { return Get<string>(OwinConstants.CommonKeys.Scheme); }
            set { Set(OwinConstants.CommonKeys.Scheme, value); }
        }

        /// <summary>
        /// The uri host.
        /// </summary>
        public string Host
        {
            get { return Get<string>(OwinConstants.CommonKeys.Host); }
            set { Set(OwinConstants.CommonKeys.Host, value); }
        }

        /// <summary>
        /// The uri port.
        /// </summary>
        public string Port
        {
            get { return Get<string>(OwinConstants.CommonKeys.Port); }
            set { Set(OwinConstants.CommonKeys.Port, value); }
        }

        /// <summary>
        /// The uri path.
        /// </summary>
        public string Path
        {
            get { return Get<string>(OwinConstants.CommonKeys.Path); }
            set { Set(OwinConstants.CommonKeys.Path, value); }
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Owin.BuilderProperties.Address"/>
        /// </summary>
        /// <returns>A new <see cref="T:Microsoft.Owin.BuilderProperties.Address" /></returns>
        public static Address Create()
        {
            return new Address(new Dictionary<string, object>());
        }

        #region Value-type equality

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The other object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(Address other)
        {
            return Equals(_dictionary, other._dictionary);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is Address && Equals((Address)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return (_dictionary != null ? _dictionary.GetHashCode() : 0);
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="T:Microsoft.Owin.BuilderProperties.Address" /> are equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if left and right represent the same address; otherwise, false.</returns>
        public static bool operator ==(Address left, Address right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="T:Microsoft.Owin.BuilderProperties.Address" /> are not equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if left and right do not represent the same address; otherwise, false.</returns>
        public static bool operator !=(Address left, Address right)
        {
            return !left.Equals(right);
        }

        #endregion

        /// <summary>
        /// Gets a specified key and value from the underlying dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            object value;
            return _dictionary.TryGetValue(key, out value) ? (T)value : default(T);
        }

        /// <summary>
        /// Sets a specified key and value in the underlying dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public Address Set(string key, object value)
        {
            _dictionary[key] = value;
            return this;
        }
    }
}
