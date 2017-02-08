// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Owin.BuilderProperties
{
    /// <summary>
    /// Represents the capabilities for the builder properties.
    /// </summary>
    public struct Capabilities
    {
        private readonly IDictionary<string, object> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Owin.BuilderProperties.Capabilities" /> class.
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
        /// Gets or sets the string value for "sendfile.Version"
        /// </summary>
        /// <returns>the string value for "sendfile.Version"</returns>
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
        /// Gets or sets the websocket version.
        /// </summary>
        /// <returns>The websocket version.</returns>
        public string WebSocketVersion
        {
            get { return Get<string>(OwinConstants.WebSocket.Version); }
            set { Set(OwinConstants.WebSocket.Version, value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Owin.BuilderProperties.Capabilities" /> class.
        /// </summary>
        /// <returns>A new instance of the <see cref="T:Microsoft.Owin.BuilderProperties.Capabilities" /> class.</returns>
        public static Capabilities Create()
        {
            return new Capabilities(new Dictionary<string, object>());
        }

        #region Value-type equality

        /// <summary>
        /// Determines whether the current Capabilities instance is equal to the specified Capabilities.
        /// </summary>
        /// <param name="other">The other Capabilities to compare with the current instance.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(Capabilities other)
        {
            return Equals(_dictionary, other._dictionary);
        }

        /// <summary>
        /// Determines whether the current Capabilities is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>true if the current Capabilities is equal to the specified object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is Capabilities && Equals((Capabilities)obj);
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
        /// Determines whether two specified instances of <see cref="T:Microsoft.Owin.BuilderProperties.Capabilities" /> are equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if the two specified instances of <see cref="T:Microsoft.Owin.BuilderProperties.Capabilities" /> are equal; otherwise, false.</returns>
        public static bool operator ==(Capabilities left, Capabilities right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="T:Microsoft.Owin.BuilderProperties.Capabilities" /> are not equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if the two specified instances of <see cref="T:Microsoft.Owin.BuilderProperties.Capabilities" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(Capabilities left, Capabilities right)
        {
            return !left.Equals(right);
        }

        #endregion

        /// <summary>
        /// Gets the value from the dictionary with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value with the specified key.</returns>
        public T Get<T>(string key)
        {
            object value;
            return _dictionary.TryGetValue(key, out value) ? (T)value : default(T);
        }

        /// <summary>
        /// Sets the given key and value in the underlying dictionary.
        /// </summary>
        /// <param name="key">The key of the value to set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>This instance.</returns>
        public Capabilities Set(string key, object value)
        {
            _dictionary[key] = value;
            return this;
        }
    }
}
