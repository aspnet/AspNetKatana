// <copyright file="AccessTokenRequest.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Specialized;

namespace Microsoft.Owin.Security.OAuth.Messages
{
    public class AccessTokenRequest
    {
        public AccessTokenRequest(IDictionary<string, string[]> parameters)
        {
            foreach (var kv in parameters)
            {
                Set(kv.Key, string.Join(",", kv.Value));
            }
        }

        public AccessTokenRequest(NameValueCollection parameters)
        {
            foreach (var key in parameters.AllKeys)
            {
                Set(key, parameters.Get(key));
            }
        }

        public string GrantType { get; set; }
        public string Code { get; set; }
        public string RedirectUri { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        private void Set(string name, string value)
        {
            if (string.Equals(name, "grant_type", StringComparison.Ordinal))
            {
                GrantType = value;
            }
            else if (string.Equals(name, "code", StringComparison.Ordinal))
            {
                Code = value;
            }
            else if (string.Equals(name, "redirect_uri", StringComparison.Ordinal))
            {
                RedirectUri = value;
            }
            else if (string.Equals(name, "client_id", StringComparison.Ordinal))
            {
                ClientId = value;
            }
        }
    }
}
