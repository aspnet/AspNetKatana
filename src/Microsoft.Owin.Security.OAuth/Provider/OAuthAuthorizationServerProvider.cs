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

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthAuthorizationServerProvider : IOAuthAuthorizationServerProvider
    {
        public OAuthAuthorizationServerProvider()
        {
            OnLookupClientId = async context => { };
            OnAuthorizeEndpoint = async context => { };
            OnTokenEndpoint = async context => { };
        }

        public Func<OAuthLookupClientIdContext, Task> OnLookupClientId { get; set; }
        public Func<OAuthAuthorizeEndpointContext, Task> OnAuthorizeEndpoint { get; set; }
        public Func<OAuthTokenEndpointContext, Task> OnTokenEndpoint { get; set; }

        public Task LookupClientId(OAuthLookupClientIdContext context)
        {
            return OnLookupClientId.Invoke(context);
        }

        public virtual Task AuthorizeEndpoint(OAuthAuthorizeEndpointContext context)
        {
            return OnAuthorizeEndpoint.Invoke(context);
        }

        public Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            return OnTokenEndpoint.Invoke(context);
        }
    }
}
