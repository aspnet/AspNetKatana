// <copyright file="OAuthTokenEndpointContext.cs" company="Microsoft Open Technologies, Inc.">
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

#if DEBUG

using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Owin.Security.OAuth.Messages;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthTokenEndpointContext : EndpointContext
    {
        public OAuthTokenEndpointContext(
            IDictionary<string, object> environment,
            AuthenticationTicket ticket,
            AccessTokenRequest accessTokenRequest) : base(environment)
        {
            Identity = ticket.Identity;
            Extra = ticket.Extra;
            AccessTokenRequest = accessTokenRequest;
            TokenIssued = Identity != null;
        }

        public ClaimsIdentity Identity { get; private set; }
        public AuthenticationExtra Extra { get; private set; }

        public AccessTokenRequest AccessTokenRequest { get; set; }

        public bool TokenIssued { get; private set; }

        public void Issue(ClaimsIdentity identity, AuthenticationExtra extra)
        {
            Identity = identity;
            Extra = extra;
            TokenIssued = true;
        }
    }
}

#endif
