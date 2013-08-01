// <copyright file="JwtSecureDataHandler.cs" company="Microsoft Open Technologies, Inc.">
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
    /// <summary>
    /// Signs and validates JSON Web Tokens.
    /// </summary>
    public class JwtSecureDataHandler : ISecureDataHandler<AuthenticationTicket>
    {
        /// <summary>
        /// The property key name for the target audience when protecting an authentication ticket.
        /// </summary>
        public const string AudiencePropertyKey = "audience";

        private const string IssuedAtClaimName = "iat";

        private const string ExpiryClaimName = "exp";

        private const string JwtIdClaimName = "jti";

        private static DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly List<string> _allowedAudiences = new List<string>();

        private readonly Dictionary<string, IIssuerSecurityTokenProvider> _issuerCredentialProviders = new Dictionary<string, IIssuerSecurityTokenProvider>();

        private readonly ISigningCredentialsProvider _signingCredentialsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtSecureDataHandler"/> class.
        /// </summary>
        /// <param name="allowedAudience">The allowed audience for JWTs.</param>
        /// <param name="issuerSecurityTokenProvider">The issuer credential provider.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="issuerSecurityTokenProvider"/> is null.</exception>
        public JwtSecureDataHandler(string allowedAudience, IIssuerSecurityTokenProvider issuerSecurityTokenProvider)            
        {
            if (string.IsNullOrWhiteSpace(allowedAudience))
            {
                throw new ArgumentNullException("allowedAudience");
            }

            if (issuerSecurityTokenProvider == null)
            {
                throw new ArgumentNullException("issuerSecurityTokenProvider");
            }

            ValidateIssuer = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtSecureDataHandler"/> class.
        /// </summary>
        /// <param name="allowedAudiences">The allowed audience for JWTs.</param>
        /// <param name="issuerCredentialProviders">The issuer credential provider.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="issuerCredentialProviders"/> is null.</exception>
        public JwtSecureDataHandler(IEnumerable<string> allowedAudiences, IEnumerable<IIssuerSecurityTokenProvider> issuerCredentialProviders)
        {
            if (allowedAudiences == null)
            {
                throw new ArgumentNullException("allowedAudiences");
            }

            var audiences = new List<string>(allowedAudiences);
            if (!audiences.Any())
            {
                throw new ArgumentOutOfRangeException("allowedAudiences", Properties.Resources.Exception_AudiencesMustBeSpecified);
            }

            _allowedAudiences.AddRange(audiences);

            if (issuerCredentialProviders == null)
            {
                throw new ArgumentNullException("issuerCredentialProviders");
            }

            var credentialProviders = new List<IIssuerSecurityTokenProvider>(issuerCredentialProviders);
            if (!credentialProviders.Any())
            {
                throw new ArgumentOutOfRangeException("issuerCredentialProviders", Properties.Resources.Exception_IssuerCredentialProvidersMustBeSpecified);
            }

            foreach (var issuerCredentialProvider in credentialProviders)
            {
                _issuerCredentialProviders.Add(issuerCredentialProvider.Issuer, issuerCredentialProvider);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtSecureDataHandler"/> class which supports JWT generation.
        /// </summary>
        /// <param name="signingCredentialsProvider">The signing credentials provider.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="signingCredentialsProvider"/> is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The <paramref name="signingCredentialsProvider"/> does not have a valid issuer.</exception>
        public JwtSecureDataHandler(ISigningCredentialsProvider signingCredentialsProvider)
        {            
            if (signingCredentialsProvider == null)
            {
                throw new ArgumentNullException("signingCredentialsProvider");
            }

            if (string.IsNullOrWhiteSpace(signingCredentialsProvider.Issuer))
            {
                throw new ArgumentOutOfRangeException("signingCredentialsProvider", Properties.Resources.Exception_SigningCredentialsProviderMustHaveAnIssuer);
            }

            _signingCredentialsProvider = signingCredentialsProvider;
            _allowedAudiences.Add(signingCredentialsProvider.Issuer);
            _issuerCredentialProviders.Add(signingCredentialsProvider.Issuer, signingCredentialsProvider);
            ValidateIssuer = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtSecureDataHandler"/> class.
        /// </summary>
        /// <param name="allowedAudiences">The allowed audiences for inbound JWT parsing.</param>
        /// <param name="issuerCredentialProviders">The issuer credential providers for inbound JWT parsing.</param>
        /// <param name="signingCredentialsProvider">The signing credentials provider to enable JWT generation.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="signingCredentialsProvider"/> is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The <paramref name="signingCredentialsProvider"/> does not have a valid issuer.</exception>
        public JwtSecureDataHandler(IEnumerable<string> allowedAudiences, IEnumerable<IIssuerSecurityTokenProvider> issuerCredentialProviders, ISigningCredentialsProvider signingCredentialsProvider)
            : this(allowedAudiences, issuerCredentialProviders)
        {
            if (signingCredentialsProvider == null)
            {
                throw new ArgumentNullException("signingCredentialsProvider");
            }

            if (string.IsNullOrWhiteSpace(signingCredentialsProvider.Issuer))
            {
                throw new ArgumentOutOfRangeException("signingCredentialsProvider", Properties.Resources.Exception_SigningCredentialsProviderMustHaveAnIssuer);
            }

            _signingCredentialsProvider = signingCredentialsProvider;
            _allowedAudiences.Add(signingCredentialsProvider.Issuer);
            _issuerCredentialProviders.Add(signingCredentialsProvider.Issuer, signingCredentialsProvider);
            ValidateIssuer = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether JWT issuers should be validated.
        /// </summary>
        /// <value>
        /// true if the issuer should be validate; otherwise, false.
        /// </value>
        public bool ValidateIssuer
        {
            get;
            set;
        }

        /// <summary>
        /// Transforms the specified authentication ticket into a JWT.
        /// </summary>
        /// <param name="data">The authentication ticket to transform into a JWT.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="data"/> is null.</exception>
        /// <exception cref="System.NotSupportedException">Thrown if the IssuingSecurityTokenProvider is not a SigningSecurityTokenProvider.</exception>
        public string Protect(AuthenticationTicket data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (_signingCredentialsProvider == null)
            {
                throw new NotSupportedException(Properties.Resources.Exception_CannotSign);
            }

            string audience = data.Extra.Properties.ContainsKey(AudiencePropertyKey) ? data.Extra.Properties[AudiencePropertyKey] : null;

            // As JWT doesn't have a mechanism of passing metadata about what claim should be the name/subject the JWT handler
            // users the default Name claim type. If the identity has another claim type as the name type we need to 
            // switch it to the DefaultNameClaimType.
            var identity = new ClaimsIdentity(data.Identity);
            if (identity.NameClaimType != ClaimsIdentity.DefaultNameClaimType && !string.IsNullOrWhiteSpace(identity.Name))
            {
                identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, identity.Name));
                identity.RemoveClaim(identity.Claims.First(c => c.Type == identity.NameClaimType));
            }

            // And now do the same for roles.
            var roleClaims = identity.Claims.Where(c => c.Type == identity.RoleClaimType).ToList();
            if (identity.RoleClaimType != ClaimsIdentity.DefaultRoleClaimType && roleClaims.Any())
            {
                foreach (var roleClaim in roleClaims)
                {
                    identity.RemoveClaim(roleClaim);
                    identity.AddClaim(new Claim(ClaimsIdentity.DefaultRoleClaimType, roleClaim.Value, roleClaim.ValueType, roleClaim.Issuer, roleClaim.OriginalIssuer));
                }
            }

            identity.AddClaims(new[]
                {
                    new Claim(IssuedAtClaimName, GetEpocTimeStamp()), 
                    new Claim(JwtIdClaimName, Guid.NewGuid().ToString("N"))
                });                        

            Lifetime lifetime = null;
            if (data.Extra.IssuedUtc != null || data.Extra.ExpiresUtc != null)
            {
                lifetime = new Lifetime(data.Extra.IssuedUtc != null ? (DateTime?)((DateTimeOffset)data.Extra.IssuedUtc).UtcDateTime : null, data.Extra.ExpiresUtc != null ? (DateTime?)((DateTimeOffset)data.Extra.ExpiresUtc).UtcDateTime : null);
            }

            var handler = new JwtSecurityTokenHandler();

            JwtSecurityToken jwt = handler.CreateToken(_signingCredentialsProvider.Issuer, audience, identity, lifetime, _signingCredentialsProvider.SigningCredentials);

            return jwt.RawData;
        }

        /// <summary>
        /// Validates the specified JWT Token and builds an AuthenticationTicket from it.
        /// </summary>
        /// <param name="jwtToken">The JWT token to validate.</param>
        /// <returns>An AuthenticationTicket built from the <paramref name="jwtToken"/></returns>
        /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="jwtToken"/> is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the <paramref name="jwtToken"/> is not a JWT token.</exception>
        public AuthenticationTicket Unprotect(string jwtToken)
        {
            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                throw new ArgumentNullException("jwtToken");
            }

            var handler = new JwtSecurityTokenHandler
            {
                CertificateValidator = X509CertificateValidator.None
            };

            var token = handler.ReadToken(jwtToken) as JwtSecurityToken;

            if (token == null)
            {
                throw new ArgumentOutOfRangeException("jwtToken", Properties.Resources.Exception_InvalidJwt);
            }

            var validationParameters = new TokenValidationParameters { AllowedAudiences = _allowedAudiences, ValidateIssuer = ValidateIssuer };

            if (ValidateIssuer)
            {
                if (string.IsNullOrWhiteSpace(token.Issuer))
                {
                    throw new ArgumentOutOfRangeException("jwtToken", Properties.Resources.Exception_CannotValidateIssuer);
                }

                if (!_issuerCredentialProviders.ContainsKey(token.Issuer))
                {
                    throw new SecurityTokenException(Properties.Resources.Exception_UnknownIssuer);
                }

                validationParameters.ValidIssuers = _issuerCredentialProviders.Keys;
            }

            var signingTokens = new List<SecurityToken>();
            if (ValidateIssuer)
            {
                signingTokens.AddRange(_issuerCredentialProviders[token.Issuer].SecurityTokens);
            }
            else
            {
                foreach (var issuerSecurityTokenProvider in _issuerCredentialProviders)
                {
                    signingTokens.AddRange(issuerSecurityTokenProvider.Value.SecurityTokens);
                }
            }

            validationParameters.SigningTokens = signingTokens;

            ClaimsPrincipal claimsPrincipal = handler.ValidateToken(jwtToken, validationParameters);
            var claimsIdentity = (ClaimsIdentity)claimsPrincipal.Identity;

            // Fill out the authenticationExtra issued and expires times if the equivalent claims are in the JWT
            var authenticationExtra = new AuthenticationExtra(new Dictionary<string, string>());
            if (claimsIdentity.Claims.Any(c => c.Type == ExpiryClaimName))
            {
                var expiryClaim = (from c in claimsIdentity.Claims where c.Type == ExpiryClaimName select c.Value).Single();
                authenticationExtra.ExpiresUtc = _epoch.AddSeconds(Convert.ToInt64(expiryClaim, CultureInfo.InvariantCulture));
            }

            if (claimsIdentity.Claims.Any(c => c.Type == IssuedAtClaimName))
            {
                var issued = (from c in claimsIdentity.Claims where c.Type == IssuedAtClaimName select c.Value).Single();
                authenticationExtra.IssuedUtc = _epoch.AddSeconds(Convert.ToInt64(issued, CultureInfo.InvariantCulture));
            }

            // Finally, create a new ClaimsIdentity so the auth type is JWT rather than Federated.
            var returnedIdentity = new ClaimsIdentity(claimsIdentity.Claims, "JWT");

            return new AuthenticationTicket(returnedIdentity, authenticationExtra);
        }

        private static string GetEpocTimeStamp()
        {
            var secondsSinceUnixEpocStart = DateTime.UtcNow - _epoch;
            return Convert.ToInt64(secondsSinceUnixEpocStart.TotalSeconds).ToString(CultureInfo.InvariantCulture);            
        }
    }
}
