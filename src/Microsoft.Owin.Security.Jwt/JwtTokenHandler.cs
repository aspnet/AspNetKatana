// <copyright file="JwtTokenHandler.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Globalization;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;

namespace Microsoft.Owin.Security.Jwt
{
    public class JwtTokenHandler : ISecureDataHandler<AuthenticationTicket>
    {
        public const string AudiencePropertyKey = "audience";

        private const string IssuedAtClaimName = "iat";

        private const string JwtIdClaimName = "jti";

        private ISecurityTokenProvider _securityTokenProvider;

        private static DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public JwtTokenHandler(ISecurityTokenProvider securityTokenProvider)
        {
            if (securityTokenProvider == null)
            {
                throw new ArgumentNullException("securityTokenProvider");
            }

            _securityTokenProvider = securityTokenProvider;
        }

        public bool CanSign
        {
            get
            {
                var signingSecurityTokenProvider = _securityTokenProvider as ISigningSecurityTokenProvider;
                return signingSecurityTokenProvider != null;
            }
        }

        public string Protect(AuthenticationTicket data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            var signingSecurityTokenProvider = _securityTokenProvider as ISigningSecurityTokenProvider;

            if (signingSecurityTokenProvider == null)
            {
                throw new NotSupportedException();
            }

            var claimsIdentity = new ClaimsIdentity(data.Identity);
            claimsIdentity.AddClaims(new[]
                {
                    new Claim(IssuedAtClaimName, GetEpocTimeStamp()), 
                    new Claim(JwtIdClaimName, Guid.NewGuid().ToString("N"))
                });

            string audience = data.Extra.Properties.ContainsKey(AudiencePropertyKey) ? data.Extra.Properties[AudiencePropertyKey] : null;

            Lifetime lifetime = null;
            if (data.Extra.IssuedUtc != null || data.Extra.ExpiresUtc != null)
            {
                lifetime = new Lifetime(data.Extra.IssuedUtc != null ? (DateTime?)((DateTimeOffset)data.Extra.IssuedUtc).UtcDateTime : null, data.Extra.ExpiresUtc != null ? (DateTime?)((DateTimeOffset)data.Extra.ExpiresUtc).UtcDateTime : null);
            }

            var handler = new JwtSecurityTokenHandler();

            JwtSecurityToken jwt = handler.CreateToken(signingSecurityTokenProvider.Issuer, audience, claimsIdentity, lifetime, signingSecurityTokenProvider.SigningCredentials);

            return jwt.RawData;
        }

        public AuthenticationTicket Unprotect(string protectedText)
        {
            if (string.IsNullOrWhiteSpace(protectedText))
            {
                throw new ArgumentNullException("protectedText");
            }

            var handler = new JwtSecurityTokenHandler()
            {
                CertificateValidator = X509CertificateValidator.None
            };

            var token = handler.ReadToken(protectedText) as JwtSecurityToken;

            if (token == null)
            {
                throw new ArgumentOutOfRangeException("protectedText", Properties.Resources.Exception_InvalidJwt);
            }

            var validationParameters = new TokenValidationParameters();
            if ((_securityTokenProvider.ValidationParameters & JwtValidationParameters.Issuer) == JwtValidationParameters.Issuer)
            {
                if (string.IsNullOrWhiteSpace(token.Issuer))
                {
                    throw new ArgumentOutOfRangeException("protectedText", Properties.Resources.Exception_CannotValidateIssuer);
                }
                validationParameters.SigningTokens = new List<SecurityToken> { _securityTokenProvider.GetSigningTokenForIssuer(token.Issuer) };
                validationParameters.ValidIssuer = token.Issuer;
            }
            else
            {
                validationParameters.SigningTokens = _securityTokenProvider.GetSigningTokens();
            }

            validationParameters.AllowedAudiences = _securityTokenProvider.ExpectedAudiences;

            ClaimsPrincipal claimsPrincipal = handler.ValidateToken(protectedText, validationParameters);
            var claimsIdentity = (ClaimsIdentity)claimsPrincipal.Identity;

            var authenticationExtra = new AuthenticationExtra(new Dictionary<string, string>());

            if (claimsIdentity.Claims.Any(c => c.Type == "exp"))
            {
                var expiryClaim = (from c in claimsIdentity.Claims where c.Type == "exp" select c.Value).Single();
                authenticationExtra.ExpiresUtc = _epoch.AddSeconds(Convert.ToInt64(expiryClaim, CultureInfo.InvariantCulture));
            }

            if (claimsIdentity.Claims.Any(c => c.Type == "iat"))
            {
                var issued = (from c in claimsIdentity.Claims where c.Type == "iat" select c.Value).Single();
                authenticationExtra.IssuedUtc = _epoch.AddSeconds(Convert.ToInt64(issued, CultureInfo.InvariantCulture));
            }

            return new AuthenticationTicket(claimsIdentity, authenticationExtra);
        }

        private static string GetEpocTimeStamp()
        {
            var secondsSinceUnixEpocStart = DateTime.UtcNow - _epoch;
            return Convert.ToInt64(secondsSinceUnixEpocStart.TotalSeconds).ToString(CultureInfo.InvariantCulture);            
        }
    }
}
