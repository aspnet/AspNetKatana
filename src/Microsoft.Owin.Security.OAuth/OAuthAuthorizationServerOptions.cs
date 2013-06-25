// <copyright file="OAuthAuthorizationServerOptions.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security.DataProtection;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthAuthorizationServerOptions : AuthenticationOptions
    {
        public OAuthAuthorizationServerOptions() : base("Bearer")
        {
            AuthenticationCodeExpireTimeSpan = TimeSpan.FromMinutes(5);
            AccessTokenExpireTimeSpan = TimeSpan.FromDays(14);
            SystemClock = new SystemClock();
        }

        public string AuthorizeEndpointPath { get; set; }
        public string TokenEndpointPath { get; set; }

        public IOAuthAuthorizationServerProvider Provider { get; set; }

        public ISecureDataHandler<AuthenticationTicket> AuthenticationCodeHandler { get; set; }
        public ISecureDataHandler<AuthenticationTicket> AccessTokenHandler { get; set; }

        public TimeSpan AuthenticationCodeExpireTimeSpan { get; set; }
        public TimeSpan AccessTokenExpireTimeSpan { get; set; }

        public ISystemClock SystemClock { get; set; }
    }
}

#endif
