// <copyright file="WindowsAzureJwtTokenHandler.cs" company="Microsoft Open Technologies, Inc.">
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
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;

namespace Microsoft.Owin.Security.WindowsAzure
{
    public class WindowsAzureJwtTokenHandler : ISecureDataHandler<AuthenticationTicket>
    {
        private static DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public WindowsAzureJwtTokenHandler(string tenant, string audience, IMetadataResolver metadataResolver)
        {
            if (string.IsNullOrWhiteSpace(tenant))
            {
                throw new ArgumentNullException("tenant");
            }

            if (metadataResolver == null)
            {
                throw new ArgumentNullException("metadataResolver");
            }

            Audience = audience;
            MetadataResolver = metadataResolver;
            Tenant = tenant;
        }

        public string Tenant { get; private set; }

        public string Audience { get; private set; }

        public IMetadataResolver MetadataResolver { get; private set; }

        public string Protect(AuthenticationTicket data)
        {
            throw new NotImplementedException();
        }

        public AuthenticationTicket Unprotect(string protectedText)
        {
            var expectedIssuer = MetadataResolver.GetIssuer(Tenant);
            var validSigningTokens = MetadataResolver.GetSigningTokens(Tenant);

            var tokenHandler = new JwtSecurityTokenHandler()
            {
                CertificateValidator = X509CertificateValidator.None
            };
            
            var validationParameters = new TokenValidationParameters
            {
                AllowedAudience = Audience,
                ValidIssuer = expectedIssuer,
                SigningTokens = validSigningTokens
            };

            ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(protectedText, validationParameters);
            var claimsIdentity = (ClaimsIdentity)claimsPrincipal.Identity;            

            var authenticationExtra = new AuthenticationExtra(new Dictionary<string, string>());

            if (claimsIdentity.Claims.Any(c => c.Type == "exp"))
            {
                var expiryClaim = (from c in claimsIdentity.Claims where c.Type == "exp" select c.Value).Single();
                authenticationExtra.ExpiresUtc = _epoch.AddSeconds(Convert.ToInt64(expiryClaim));
            }

            if (claimsIdentity.Claims.Any(c => c.Type == "iat"))
            {
                var issued = (from c in claimsIdentity.Claims where c.Type == "iat" select c.Value).Single();
                authenticationExtra.IssuedUtc = _epoch.AddSeconds(Convert.ToInt64(issued));
            }

            return new AuthenticationTicket(claimsIdentity, authenticationExtra);
        }
    }
}
