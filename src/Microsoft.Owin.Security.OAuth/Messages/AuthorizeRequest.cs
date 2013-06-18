// <copyright file="AuthorizeRequest.cs" company="Microsoft Open Technologies, Inc.">
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

#if AUTHSERVER

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security.OAuth.Messages
{
    public class AuthorizeRequest
    {
        public AuthorizeRequest(IReadableStringCollection parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            foreach (var kv in parameters)
            {
                AddParameter(kv.Key, string.Join(",", kv.Value));
            }
        }

        public string ResponseType { get; set; }
        public string ClientId { get; set; }
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "By design")]
        public string RedirectUri { get; set; }
        public string Scope { get; set; }
        public string State { get; set; }

        public bool ResponseTypeIsCode
        {
            get { return string.Equals(ResponseType, "code", StringComparison.Ordinal); }
        }
        public bool ResponseTypeIsToken
        {
            get { return string.Equals(ResponseType, "token", StringComparison.Ordinal); }
        }

        private void AddParameter(string name, string value)
        {
            if (string.Equals(name, "response_type", StringComparison.Ordinal))
            {
                ResponseType = value;
            }
            else if (string.Equals(name, "client_id", StringComparison.Ordinal))
            {
                ClientId = value;
            }
            else if (string.Equals(name, "redirect_uri", StringComparison.Ordinal))
            {
                RedirectUri = value;
            }
            else if (string.Equals(name, "scope", StringComparison.Ordinal))
            {
                Scope = value;
            }
            else if (string.Equals(name, "state", StringComparison.Ordinal))
            {
                State = value;
            }
        }
    }
}

#endif
