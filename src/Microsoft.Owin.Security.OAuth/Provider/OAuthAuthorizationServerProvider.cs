// <copyright file="OAuthAuthorizationServerProvider.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthAuthorizationServerProvider : IOAuthAuthorizationServerProvider
    {
        public OAuthAuthorizationServerProvider()
        {
            OnValidateClientCredentials = context => Task.FromResult<object>(null);
            OnValidateResourceOwnerCredentials = context => Task.FromResult<object>(null);
            OnAuthorizeEndpoint = context => Task.FromResult<object>(null);
            OnTokenEndpoint = context => Task.FromResult<object>(null);
        }

        public Func<OAuthValidateClientCredentialsContext, Task> OnValidateClientCredentials { get; set; }
        public Func<OAuthValidateResourceOwnerCredentialsContext, Task> OnValidateResourceOwnerCredentials { get; set; }
        public Func<OAuthAuthorizeEndpointContext, Task> OnAuthorizeEndpoint { get; set; }
        public Func<OAuthTokenEndpointContext, Task> OnTokenEndpoint { get; set; }

        public virtual Task ValidateClientCredentials(OAuthValidateClientCredentialsContext context)
        {
            return OnValidateClientCredentials.Invoke(context);
        }

        public virtual Task ValidateResourceOwnerCredentials(OAuthValidateResourceOwnerCredentialsContext context)
        {
            return OnValidateResourceOwnerCredentials.Invoke(context);
        }

        public virtual Task AuthorizeEndpoint(OAuthAuthorizeEndpointContext context)
        {
            return OnAuthorizeEndpoint.Invoke(context);
        }

        public virtual Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            return OnTokenEndpoint.Invoke(context);
        }
    }
}

#endif
