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
using System.IO;
using System.Security.Principal;
using Owin.Types.Extensions;

namespace Microsoft.Owin
{
    public partial struct OwinRequest
    {
        private global::Owin.Types.OwinRequest _request;

        public OwinRequest(IDictionary<string, object> environment)
        {
            _request = new global::Owin.Types.OwinRequest(environment);
        }

        public IDictionary<string, object> Environment
        {
            get { return _request.Dictionary; }
        }

        public string Scheme
        {
            get { return _request.Scheme; }
            set { _request.Scheme = value; }
        }

        public string Host
        {
            get { return _request.Host; }
            set { _request.Host = value; }
        }

        public string PathBase
        {
            get { return _request.PathBase; }
            set { _request.PathBase = value; }
        }

        public string Path
        {
            get { return _request.Path; }
            set { _request.Path = value; }
        }

        public string QueryString
        {
            get { return _request.QueryString; }
            set { _request.QueryString = value; }
        }

        public IPrincipal User
        {
            get { return _request.User; }
            set { _request.User = value; }
        }

        public string Method
        {
            get { return _request.Method; }
            set { _request.Method = value; }
        }

        public Stream Body
        {
            get { return _request.Body; }
            set { _request.Body = value; }
        }

        public Uri Uri
        {
            get { return _request.Uri; }
        }

        public T Get<T>(string key)
        {
            return _request.Get<T>(key);
        }

        public void Set<T>(string key, T value)
        {
            _request.Set(key, value);
        }

        public void OnSendingHeaders(Action<object> callback, object state)
        {
            _request.OnSendingHeaders(callback, state);
        }

        public void OnSendingHeaders(Action callback)
        {
            _request.OnSendingHeaders(state => ((Action)state).Invoke(), callback);
        }

        public IDictionary<string, string> GetCookies()
        {
            return _request.GetCookies();
        }

        public IDictionary<string, string[]> GetQuery()
        {
            return _request.GetQuery();
        }

        public static OwinRequest Create()
        {
            return new OwinRequest(global::Owin.Types.OwinRequest.Create().Dictionary);
        }

        public string GetHeader(string name)
        {
            return _request.GetHeader(name);
        }
    }
}
