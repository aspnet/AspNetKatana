// <copyright file="AppProperties.cs" company="Microsoft Open Technologies, Inc.">
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.BuilderProperties
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public struct AppProperties
    {
        private readonly IDictionary<string, object> _dictionary;

        public AppProperties(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        // owin.Version 1.0
        public string OwinVersion
        {
            get { return Get<string>(OwinConstants.OwinVersion); }
            set { Set(OwinConstants.OwinVersion, value); }
        }

        // builder.DefaultApp AppFunc (404)
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public AppFunc DefaultApp
        {
            get { return Get<AppFunc>(OwinConstants.Builder.DefaultApp); }
            set { Set(OwinConstants.Builder.DefaultApp, value); }
        }

        // builder.AddSignatureConversion Action<Delegate>
        public Action<Delegate> AddSignatureConversionDelegate
        {
            get { return Get<Action<Delegate>>(OwinConstants.Builder.AddSignatureConversion); }
            set { Set(OwinConstants.Builder.AddSignatureConversion, value); }
        }

        // host.AppName string
        public string AppName
        {
            get { return Get<string>(OwinConstants.CommonKeys.AppName); }
            set { Set(OwinConstants.CommonKeys.AppName, value); }
        }

        // host.TraceOutput TextWriter
        public TextWriter TraceOutput
        {
            get { return Get<TextWriter>(OwinConstants.CommonKeys.TraceOutput); }
            set { Set(OwinConstants.CommonKeys.TraceOutput, value); }
        }

        // host.OnAppDisposing CancellationToken
        public CancellationToken OnAppDisposing
        {
            get { return Get<CancellationToken>(OwinConstants.CommonKeys.OnAppDisposing); }
            set { Set(OwinConstants.CommonKeys.OnAppDisposing, value); }
        }

        // host.Addresses IList<IDictionary<string, object>>
        public AddressCollection Addresses
        {
            get { return new AddressCollection(Get<IList<IDictionary<string, object>>>(OwinConstants.CommonKeys.Addresses)); }
            set { Set(OwinConstants.CommonKeys.Addresses, value.List); }
        }

        // server.Capabilities IDictionary<string, object>
        public Capabilities Capabilities
        {
            get { return new Capabilities(Get<IDictionary<string, object>>(OwinConstants.CommonKeys.Capabilities)); }
            set { Set(OwinConstants.CommonKeys.Capabilities, value.Dictionary); }
        }

        // TODO: host.TraceSource TraceSource?

        public IDictionary<string, object> Dictionary
        {
            get { return _dictionary; }
        }

#region Value-type equality
        public bool Equals(AppProperties other)
        {
            return Equals(_dictionary, other._dictionary);
        }

        public override bool Equals(object obj)
        {
            return obj is AppProperties && Equals((AppProperties)obj);
        }

        public override int GetHashCode()
        {
            return (_dictionary != null ? _dictionary.GetHashCode() : 0);
        }

        public static bool operator ==(AppProperties left, AppProperties right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AppProperties left, AppProperties right)
        {
            return !left.Equals(right);
        }
#endregion

        public T Get<T>(string key)
        {
            object value;
            return _dictionary.TryGetValue(key, out value) ? (T)value : default(T);
        }

        public AppProperties Set(string key, object value)
        {
            _dictionary[key] = value;
            return this;
        }
    }
}
