// <copyright file="OwinResponse.cs" company="Microsoft Open Technologies, Inc.">
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
using System.IO;
using Owin.Types.Extensions;

namespace Microsoft.Owin
{
    /// <summary>
    /// This wraps OWIN environment dictionary and provides strongly typed accessors.
    /// </summary>
    public partial struct OwinResponse
    {
        private global::Owin.Types.OwinResponse _response;

        /// <summary>
        /// Creates a new environment wrapper exposing response properties.
        /// </summary>
        /// <param name="environment"></param>
        public OwinResponse(IDictionary<string, object> environment)
        {
            _response = new global::Owin.Types.OwinResponse(environment);
        }

        /// <summary>
        /// Creates a new environment wrapper exposing response properties.
        /// </summary>
        /// <param name="request"></param>
        public OwinResponse(OwinRequest request) : this(request.Environment)
        {
        }

        /// <summary>
        /// The wrapped OWIN environment.
        /// </summary>
        public IDictionary<string, object> Environment
        {
            get { return _response.Dictionary; }
        }

        /// <summary>
        /// The optional owin.ResponseStatsCode.
        /// </summary>
        public int StatusCode
        {
            get { return _response.StatusCode; }
            set { _response.StatusCode = value; }
        }

        /// <summary>
        /// The Content-Type response header.
        /// </summary>
        public string ContentType
        {
            get { return _response.ContentType; }
            set { _response.ContentType = value; }
        }

        /// <summary>
        /// The owin.ResponseBody Stream.
        /// </summary>
        public Stream Body
        {
            get { return _response.Body; }
            set { _response.Body = value; }
        }

        /// <summary>
        /// Appends a cookie to the response Set-Cookie header.
        /// </summary>
        /// <param name="cookieName"></param>
        /// <param name="cookieValue"></param>
        public void AddCookie(string cookieName, string cookieValue)
        {
            _response.AddCookie(cookieName, cookieValue);
        }

        /// <summary>
        /// Appends a cookie to the response Set-Cookie header.
        /// </summary>
        /// <param name="cookieName"></param>
        /// <param name="cookieValue"></param>
        /// <param name="cookieOptions"></param>
        public void AddCookie(string cookieName, string cookieValue, CookieOptions cookieOptions)
        {
            if (cookieOptions == null)
            {
                throw new ArgumentNullException("cookieOptions");
            }
            _response.AddCookie(cookieName, cookieValue, new global::Owin.Types.Helpers.CookieOptions
            {
                Domain = cookieOptions.Domain,
                Path = cookieOptions.Path,
                Expires = cookieOptions.Expires,
                Secure = cookieOptions.Secure,
                HttpOnly = cookieOptions.HttpOnly,
            });
        }

        /// <summary>
        /// Replaces the given cookie with an expired cookie.
        /// </summary>
        /// <param name="cookieName"></param>
        public void DeleteCookie(string cookieName)
        {
            _response.DeleteCookie(cookieName);
        }

        /// <summary>
        /// Replaces the given cookie with an expired cookie.
        /// </summary>
        /// <param name="cookieName"></param>
        /// <param name="cookieOptions"></param>
        public void DeleteCookie(string cookieName, CookieOptions cookieOptions)
        {
            if (cookieOptions == null)
            {
                throw new ArgumentNullException("cookieOptions");
            }
            _response.DeleteCookie(cookieName, new global::Owin.Types.Helpers.CookieOptions
            {
                Domain = cookieOptions.Domain,
                Path = cookieOptions.Path,
                Expires = cookieOptions.Expires,
                Secure = cookieOptions.Secure,
                HttpOnly = cookieOptions.HttpOnly,
            });
        }

        /// <summary>
        /// Append a header in the owin.ResponseHeaders.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddHeader(string name, string value)
        {
            _response.AddHeader(name, value);
        }

        /// <summary>
        /// Overwrite a header in the owin.ResponseHeaders.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetHeader(string name, string value)
        {
            _response.SetHeader(name, value);
        }

        /// <summary>
        /// Sets a 302 response status code and the Location header.
        /// </summary>
        /// <param name="location"></param>
        public void Redirect(string location)
        {
            _response.Redirect(location);
        }

        #region Value-type equality

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(OwinResponse other)
        {
            return Equals(_response, other._response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is OwinResponse && Equals((OwinResponse)obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (_response != null ? _response.GetHashCode() : 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(OwinResponse left, OwinResponse right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(OwinResponse left, OwinResponse right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}
