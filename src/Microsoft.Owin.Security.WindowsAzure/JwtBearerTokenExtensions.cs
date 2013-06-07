// <copyright file="JwtBearerTokenExtensions.cs" company="Microsoft Open Technologies, Inc.">
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

using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.WindowsAzure;

namespace Owin
{
    public static class JwtBearerTokenExtensions
    {
        public static IAppBuilder UseWindowsAzureBearerToken(this IAppBuilder app, JwtBearerAuthenticationOptions options)
        {
            var bearerOptions = new OAuthBearerAuthenticationOptions
            {
                Realm = options.Realm, 
                Provider = options.Provider, 
                AccessTokenHandler = new JwtTokenHandler(options.Tenant, options.Audience, options.MetadataResolver), 
                AuthenticationMode = options.AuthenticationMode, 
                AuthenticationType = options.AuthenticationType, 
                Description = options.Description
            };

            app.UseOAuthBearerAuthentication(bearerOptions);

            return app;
        }
    }
}
