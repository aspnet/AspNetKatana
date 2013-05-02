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
    public partial struct OwinResponse
    {
        private global::Owin.Types.OwinResponse _response;

        public OwinResponse(IDictionary<string, object> environment)
        {
            _response = new global::Owin.Types.OwinResponse(environment);
        }

        public OwinResponse(OwinRequest request) : this(request.Environment)
        {
        }

        public IDictionary<string, object> Environment
        {
            get { return _response.Dictionary; }
        }

        public int StatusCode
        {
            get { return _response.StatusCode; }
            set { _response.StatusCode = value; }
        }

        public string ContentType
        {
            get { return _response.ContentType; }
            set { _response.ContentType = value; }
        }

        public Stream Body
        {
            get { return _response.Body; }
            set { _response.Body = value; }
        }

        public void AddCookie(string cookieName, string cookieValue)
        {
            _response.AddCookie(cookieName, cookieValue);
        }

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

        public void DeleteCookie(string cookieName)
        {
            _response.DeleteCookie(cookieName);
        }

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

        public void AddHeader(string name, string value)
        {
            _response.AddHeader(name, value);
        }

        public void SetHeader(string name, string value)
        {
            _response.SetHeader(name, value);
        }

        public void Redirect(string location)
        {
            _response.Redirect(location);
        }

        #region Value-type equality

        public bool Equals(OwinResponse other)
        {
            return Equals(_response, other._response);
        }

        public override bool Equals(object obj)
        {
            return obj is OwinResponse && Equals((OwinResponse)obj);
        }

        public override int GetHashCode()
        {
            return (_response != null ? _response.GetHashCode() : 0);
        }

        public static bool operator ==(OwinResponse left, OwinResponse right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OwinResponse left, OwinResponse right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}
