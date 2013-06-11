// <copyright file="OwinRequest.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Principal;
using Microsoft.Owin.Infrastructure;
using Owin.Types.Extensions;

namespace Microsoft.Owin
{
    /// <summary>
    /// This wraps OWIN environment dictionary and provides strongly typed accessors.
    /// </summary>
    public partial struct OwinRequest
    {
        private global::Owin.Types.OwinRequest _request;

        /// <summary>
        /// Create a new environment wrapper exposing request properties.
        /// </summary>
        /// <param name="environment"></param>
        public OwinRequest(IDictionary<string, object> environment)
        {
            _request = new global::Owin.Types.OwinRequest(environment);
        }

        /// <summary>
        /// The wrapped OWIN environment.
        /// </summary>
        public IDictionary<string, object> Environment
        {
            get { return _request.Dictionary; }
        }

        /// <summary>
        /// The HTTP request scheme (e.g. http or https) from owin.RequestScheme.
        /// </summary>
        public string Scheme
        {
            get { return _request.Scheme; }
            set { _request.Scheme = value; }
        }

        /// <summary>
        /// The request host, taken from the Host request header in owin.RequestHeaders.
        /// May include the port.
        /// </summary>
        public string Host
        {
            get { return _request.Host; }
            set { _request.Host = value; }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Property accessor of collection type")]
        public IDictionary<string, string[]> Headers
        {
            get { return _request.Headers; }
            set { _request.Headers = value; }
        }

        /// <summary>
        /// See owin.RequestPathBase.
        /// </summary>
        public string PathBase
        {
            get { return _request.PathBase; }
            set { _request.PathBase = value; }
        }

        /// <summary>
        /// The request path from owin.RequestPath.
        /// </summary>
        public string Path
        {
            get { return _request.Path; }
            set { _request.Path = value; }
        }

        /// <summary>
        /// The query string from owin.RequestQueryString.
        /// </summary>
        public string QueryString
        {
            get { return _request.QueryString; }
            set { _request.QueryString = value; }
        }

        /// <summary>
        /// server.User.
        /// </summary>
        public IPrincipal User
        {
            get { return _request.User; }
            set { _request.User = value; }
        }

        /// <summary>
        /// The HTTP method/verb, e.g. GET, POST, etc..
        /// </summary>
        public string Method
        {
            get { return _request.Method; }
            set { _request.Method = value; }
        }

        /// <summary>
        /// The owin.RequestBody Stream.
        /// </summary>
        public Stream Body
        {
            get { return _request.Body; }
            set { _request.Body = value; }
        }

        /// <summary>
        /// A Uri with the combine parts of owin.RequestScheme, the Host header, owin.RequestPathBase, owin.RequestPath, and owin.RequestQueryString.
        /// </summary>
        public Uri Uri
        {
            get
            {
                // Escape things properly so System.Uri doesn't mis-interpret the data.
                string queryString = QueryString.Replace("#", "%23");
                // TODO: Measure the cost of this escaping and consider optimizing.
                string escapedPath = String.Join("/", PathBase.Split('/').Select(Uri.EscapeDataString))
                    + String.Join("/", Path.Split('/').Select(Uri.EscapeDataString));
                return string.IsNullOrWhiteSpace(queryString)
                    ? new Uri(Scheme + Uri.SchemeDelimiter + Host + escapedPath)
                    : new Uri(Scheme + Uri.SchemeDelimiter + Host + escapedPath + "?" + queryString);
            }
        }

        /// <summary>
        /// Returns true if the owin.RequestScheme is https.
        /// </summary>
        public bool IsSecure
        {
            get { return string.Equals(_request.Scheme, Constants.Https, StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>
        /// Gets a value from the OWIN environment, or returns default(T) if not present.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            return _request.Get<T>(key);
        }

        /// <summary>
        /// Sets the given key and value in the OWIN environment.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set<T>(string key, T value)
        {
            _request.Set(key, value);
        }

        /// <summary>
        /// Registers for an event that fires when the response headers are sent.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public void OnSendingHeaders(Action<object> callback, object state)
        {
            _request.OnSendingHeaders(callback, state);
        }

        /// <summary>
        /// Registers for an event that fires when the response headers are sent.
        /// </summary>
        /// <param name="callback"></param>
        public void OnSendingHeaders(Action callback)
        {
            _request.OnSendingHeaders(state => ((Action)state).Invoke(), callback);
        }

        /// <summary>
        /// Parses the Cookie header.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Does parsing")]
        public IDictionary<string, string> GetCookies()
        {
            return _request.GetCookies();
        }

        /// <summary>
        /// Parses the owin.RequestQueryString value.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Does parsing")]
        public IDictionary<string, string[]> GetQuery()
        {
            return _request.GetQuery();
        }

        /// <summary>
        /// Create a new empty OWIN environment dictionary and request wrapper.
        /// </summary>
        /// <returns></returns>
        public static OwinRequest Create()
        {
            return new OwinRequest(global::Owin.Types.OwinRequest.Create().Dictionary);
        }

        /// <summary>
        /// Gets a comma separated header, or null if the given header is not present.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetHeader(string name)
        {
            return _request.GetHeader(name);
        }

        #region Value-type equality

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(OwinRequest other)
        {
            return Equals(_request, other._request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is OwinRequest && Equals((OwinRequest)obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (_request != null ? _request.GetHashCode() : 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(OwinRequest left, OwinRequest right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(OwinRequest left, OwinRequest right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}
