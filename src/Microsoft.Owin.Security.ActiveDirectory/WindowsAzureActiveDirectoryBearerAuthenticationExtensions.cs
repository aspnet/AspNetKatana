// <copyright file="WindowsAzureJwtBearerTokenExtensions.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.ActiveDirectory.WindowsAzureActiveDirectory;
using Microsoft.Owin.Security.OAuth;

namespace Owin
{
    public static class WindowsAzureActiveDirectoryBearerAuthenticationExtensions
    {
        public static IAppBuilder UseWindowsAzureActiveDirectoryBearerAuthentication(this IAppBuilder app, WindowsAzureActiveDirectoryBearerAuthenticationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            var bearerOptions = new OAuthBearerAuthenticationOptions
            {
                Realm = options.Realm, 
                Provider = options.Provider, 
                AccessTokenFormat = new WindowsAzureJwtTokenFormat(options.Tenant, options.Audience, options.MetadataResolver), 
                AuthenticationMode = options.AuthenticationMode, 
                AuthenticationType = options.AuthenticationType, 
                Description = options.Description
            };

            app.UseOAuthBearerAuthentication(bearerOptions);

            return app;
        }
    }
}
