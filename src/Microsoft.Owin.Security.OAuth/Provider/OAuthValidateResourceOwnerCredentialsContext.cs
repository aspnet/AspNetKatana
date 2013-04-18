// <copyright file="OAuthValidateResourceOwnerCredentialsContext.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthValidateResourceOwnerCredentialsContext : BaseContext
    {
        public OAuthValidateResourceOwnerCredentialsContext(
            IDictionary<string, object> environment,
            string username,
            string password,
            string scope) : base(environment)
        {
            Username = username;
            Password = password;
            Scope = scope;
        }

        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Scope { get; private set; }

        public ClaimsIdentity Identity { get; private set; }
        public IDictionary<string, string> Extra { get; private set; }

        public bool IsValidated { get; private set; }

        public void Validated(IIdentity identity, IDictionary<string, string> extra)
        {
            Identity = identity as ClaimsIdentity ?? new ClaimsIdentity(identity);
            Extra = extra;
            IsValidated = true;
        }
    }
}
