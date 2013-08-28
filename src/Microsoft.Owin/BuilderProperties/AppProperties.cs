// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.BuilderProperties
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// A wrapper for the <see cref="P:Microsoft.Owin.Builder.AppBuilder.Properties" /> IDictionary.
    /// </summary>
    public struct AppProperties
    {
        private readonly IDictionary<string, object> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Owin.BuilderProperties.AppProperties" /> class.
        /// </summary>
        /// <param name="dictionary"></param>
        public AppProperties(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        /// <summary>
        /// Gets or sets the string value for “owin.Version”.
        /// </summary>
        /// <returns>The string value for “owin.Version”.</returns>
        public string OwinVersion
        {
            get { return Get<string>(OwinConstants.OwinVersion); }
            set { Set(OwinConstants.OwinVersion, value); }
        }

        /// <summary>
        /// Gets or sets the function delegate for “builder.DefaultApp”.
        /// </summary>
        /// <returns>The function delegate for “builder.DefaultApp”.</returns>
        public AppFunc DefaultApp
        {
            get { return Get<AppFunc>(OwinConstants.Builder.DefaultApp); }
            set { Set(OwinConstants.Builder.DefaultApp, value); }
        }

        /// <summary>
        /// Gets or sets the action delegate for “builder.AddSignatureConversion”.
        /// </summary>
        /// <returns>The action delegate for “builder.AddSignatureConversion”.</returns>
        public Action<Delegate> AddSignatureConversionDelegate
        {
            get { return Get<Action<Delegate>>(OwinConstants.Builder.AddSignatureConversion); }
            set { Set(OwinConstants.Builder.AddSignatureConversion, value); }
        }

        /// <summary>
        /// Gets or sets the string value for “host.AppName”.
        /// </summary>
        /// <returns>The string value for “host.AppName”.</returns>
        public string AppName
        {
            get { return Get<string>(OwinConstants.CommonKeys.AppName); }
            set { Set(OwinConstants.CommonKeys.AppName, value); }
        }

        /// <summary>
        /// Gets or sets the text writer for “host.TraceOutput”.
        /// </summary>
        /// <returns>The text writer for “host.TraceOutput”.</returns>
        public TextWriter TraceOutput
        {
            get { return Get<TextWriter>(OwinConstants.CommonKeys.TraceOutput); }
            set { Set(OwinConstants.CommonKeys.TraceOutput, value); }
        }

        /// <summary>
        /// Gets or sets the cancellation token for “host.OnAppDisposing”.
        /// </summary>
        /// <returns>The cancellation token for “host.OnAppDisposing”.</returns>
        public CancellationToken OnAppDisposing
        {
            get { return Get<CancellationToken>(OwinConstants.CommonKeys.OnAppDisposing); }
            set { Set(OwinConstants.CommonKeys.OnAppDisposing, value); }
        }

        /// <summary>
        /// Gets or sets the address collection for “host.Addresses”.
        /// </summary>
        /// <returns>The address collection for “host.Addresses”.</returns>
        public AddressCollection Addresses
        {
            get { return new AddressCollection(Get<IList<IDictionary<string, object>>>(OwinConstants.CommonKeys.Addresses)); }
            set { Set(OwinConstants.CommonKeys.Addresses, value.List); }
        }

        /// <summary>
        /// Gets or sets the list of “server.Capabilities”.
        /// </summary>
        /// <returns>The list of “server.Capabilities”.</returns>
        public Capabilities Capabilities
        {
            get { return new Capabilities(Get<IDictionary<string, object>>(OwinConstants.CommonKeys.Capabilities)); }
            set { Set(OwinConstants.CommonKeys.Capabilities, value.Dictionary); }
        }

        // TODO: host.TraceSource TraceSource?

        /// <summary>
        /// Gets the underlying dictionary for this <see cref="T:Microsoft.Owin.BuilderProperties.AppProperties" /> instance.
        /// </summary>
        /// <returns>The underlying dictionary for this <see cref="T:Microsoft.Owin.BuilderProperties.AppProperties" /> instance.</returns>
        public IDictionary<string, object> Dictionary
        {
            get { return _dictionary; }
        }

        #region Value-type equality

        /// <summary>
        /// Determines whether the current AppProperties is equal to the specified AppProperties.
        /// </summary>
        /// <param name="other">The other AppProperties to compare with the current instance.</param>
        /// <returns>true if the current AppProperties is equal to the specified AppProperties; otherwise, false.</returns>
        public bool Equals(AppProperties other)
        {
            return Equals(_dictionary, other._dictionary);
        }

        /// <summary>
        /// Determines whether the current AppProperties is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>true if the current AppProperties is equal to the specified object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is AppProperties && Equals((AppProperties)obj);
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
        /// Determines whether the first AppPProperties is equal to the second AppProperties.
        /// </summary>
        /// <param name="left">The first AppPropeties to compare.</param>
        /// <param name="right">The second AppPropeties to compare.</param>
        /// <returns>true if both AppProperties are equal; otherwise, false.</returns>
        public static bool operator ==(AppProperties left, AppProperties right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether the first AppPProperties is not equal to the second AppProperties.
        /// </summary>
        /// <param name="left">The first AppPropeties to compare.</param>
        /// <param name="right">The second AppPropeties to compare.</param>
        /// <returns>true if both AppProperties are not equal; otherwise, false.</returns>
        public static bool operator !=(AppProperties left, AppProperties right)
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
        /// Sets the value with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>This instance.</returns>
        public AppProperties Set(string key, object value)
        {
            _dictionary[key] = value;
            return this;
        }
    }
}
