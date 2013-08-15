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
    /// A wrapper for the IAppBuilder.Properties IDictionary
    /// </summary>
    public struct AppProperties
    {
        private readonly IDictionary<string, object> _dictionary;

        /// <summary>
        /// Create a new wrapper
        /// </summary>
        /// <param name="dictionary"></param>
        public AppProperties(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        /// <summary>
        /// owin.Version 1.0
        /// </summary>
        public string OwinVersion
        {
            get { return Get<string>(OwinConstants.OwinVersion); }
            set { Set(OwinConstants.OwinVersion, value); }
        }

        /// <summary>
        /// builder.DefaultApp AppFunc (404)
        /// </summary>
        public AppFunc DefaultApp
        {
            get { return Get<AppFunc>(OwinConstants.Builder.DefaultApp); }
            set { Set(OwinConstants.Builder.DefaultApp, value); }
        }

        /// <summary>
        /// builder.AddSignatureConversion
        /// </summary>
        public Action<Delegate> AddSignatureConversionDelegate
        {
            get { return Get<Action<Delegate>>(OwinConstants.Builder.AddSignatureConversion); }
            set { Set(OwinConstants.Builder.AddSignatureConversion, value); }
        }

        /// <summary>
        /// host.AppName string
        /// </summary>
        public string AppName
        {
            get { return Get<string>(OwinConstants.CommonKeys.AppName); }
            set { Set(OwinConstants.CommonKeys.AppName, value); }
        }

        /// <summary>
        /// host.TraceOutput TextWriter
        /// </summary>
        public TextWriter TraceOutput
        {
            get { return Get<TextWriter>(OwinConstants.CommonKeys.TraceOutput); }
            set { Set(OwinConstants.CommonKeys.TraceOutput, value); }
        }

        /// <summary>
        /// host.OnAppDisposing CancellationToken
        /// </summary>
        public CancellationToken OnAppDisposing
        {
            get { return Get<CancellationToken>(OwinConstants.CommonKeys.OnAppDisposing); }
            set { Set(OwinConstants.CommonKeys.OnAppDisposing, value); }
        }

        /// <summary>
        /// host.Addresses
        /// </summary>
        public AddressCollection Addresses
        {
            get { return new AddressCollection(Get<IList<IDictionary<string, object>>>(OwinConstants.CommonKeys.Addresses)); }
            set { Set(OwinConstants.CommonKeys.Addresses, value.List); }
        }

        /// <summary>
        /// server.Capabilities
        /// </summary>
        public Capabilities Capabilities
        {
            get { return new Capabilities(Get<IDictionary<string, object>>(OwinConstants.CommonKeys.Capabilities)); }
            set { Set(OwinConstants.CommonKeys.Capabilities, value.Dictionary); }
        }

        // TODO: host.TraceSource TraceSource?

        /// <summary>
        /// The underlying IDictionary
        /// </summary>
        public IDictionary<string, object> Dictionary
        {
            get { return _dictionary; }
        }

        #region Value-type equality

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(AppProperties other)
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
            return obj is AppProperties && Equals((AppProperties)obj);
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
        public static bool operator ==(AppProperties left, AppProperties right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(AppProperties left, AppProperties right)
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
        public AppProperties Set(string key, object value)
        {
            _dictionary[key] = value;
            return this;
        }
    }
}
