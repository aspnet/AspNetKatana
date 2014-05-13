// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;

namespace Microsoft.Owin.Security.Jwt
{
    /// <summary>
    /// Signs and validates JSON Web Tokens.
    /// </summary>
    public class JwtFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private const string IssuedAtClaimName = "iat";
        private const string ExpiryClaimName = "exp";
        private static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private TokenValidationParameters _validationParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtFormat"/> class.
        /// </summary>
        /// <param name="allowedAudience">The allowed audience for JWTs.</param>
        /// <param name="issuerCredentialProvider">The issuer credential provider.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="issuerCredentialProvider"/> is null.</exception>
        public JwtFormat(string allowedAudience, IIssuerSecurityTokenProvider issuerCredentialProvider)
        {
            if (string.IsNullOrWhiteSpace(allowedAudience))
            {
                throw new ArgumentNullException("allowedAudience");
            }
            if (issuerCredentialProvider == null)
            {
                throw new ArgumentNullException("issuerCredentialProvider");
            }

            _validationParameters = new TokenValidationParameters()
            {
                ValidAudience = allowedAudience,
                ValidIssuer = issuerCredentialProvider.Issuer,
                IssuerSigningTokens = issuerCredentialProvider.SecurityTokens.ToList(),
                ValidateIssuer = true
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtFormat"/> class.
        /// </summary>
        /// <param name="allowedAudiences">The allowed audience for JWTs.</param>
        /// <param name="issuerCredentialProviders">The issuer credential provider.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="issuerCredentialProviders"/> is null.</exception>
        public JwtFormat(IEnumerable<string> allowedAudiences, IEnumerable<IIssuerSecurityTokenProvider> issuerCredentialProviders)
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

            if (issuerCredentialProviders == null)
            {
                throw new ArgumentNullException("issuerCredentialProviders");
            }
            var credentialProviders = new List<IIssuerSecurityTokenProvider>(issuerCredentialProviders);
            if (!credentialProviders.Any())
            {
                throw new ArgumentOutOfRangeException("issuerCredentialProviders", Properties.Resources.Exception_IssuerCredentialProvidersMustBeSpecified);
            }

            _validationParameters = new TokenValidationParameters()
            {
                ValidAudiences = audiences,
                ValidIssuers = credentialProviders.Select(provider => provider.Issuer).ToList(),
                IssuerSigningTokens = credentialProviders
                    .Select(provider => provider.SecurityTokens.ToList())
                    .Aggregate((l1, l2) => { l1.AddRange(l2); return l1; }),
                ValidateIssuer = true
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtFormat"/> class.
        /// </summary>
        /// <param name="validationParameters"> <see cref="TokenValidationParameters"/> used to determine if a token is valid.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="validationParameters"/> is null.</exception>
        public JwtFormat(TokenValidationParameters validationParameters)
        {
            if (validationParameters == null)
            {
                throw new ArgumentNullException("validationParameters");
            }

            _validationParameters = validationParameters;
        }

        /// <summary>
        /// Gets or sets a value indicating whether JWT issuers should be validated.
        /// </summary>
        /// <value>
        /// true if the issuer should be validate; otherwise, false.
        /// </value>
        public bool ValidateIssuer
        {
            get { return _validationParameters.ValidateIssuer; }
            set { _validationParameters.ValidateIssuer = value; }
        }

        /// <summary>
        /// A System.IdentityModel.Tokens.SecurityTokenHandler designed for creating and validating Json Web Tokens.
        /// </summary>
        public JwtSecurityTokenHandler TokenHandler { get; set; }

        /// <summary>
        /// Transforms the specified authentication ticket into a JWT.
        /// </summary>
        /// <param name="data">The authentication ticket to transform into a JWT.</param>
        /// <returns></returns>
        public string Protect(AuthenticationTicket data)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Validates the specified JWT and builds an AuthenticationTicket from it.
        /// </summary>
        /// <param name="protectedText">The JWT to validate.</param>
        /// <returns>An AuthenticationTicket built from the <paramref name="protectedText"/></returns>
        /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="protectedText"/> is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the <paramref name="protectedText"/> is not a JWT.</exception>
        public AuthenticationTicket Unprotect(string protectedText)
        {
            if (string.IsNullOrWhiteSpace(protectedText))
            {
                throw new ArgumentNullException("protectedText");
            }

            if (TokenHandler == null)
            {
                TokenHandler = new JwtSecurityTokenHandler()
                {
                    CertificateValidator = X509CertificateValidator.None
                };
            }

            var token = TokenHandler.ReadToken(protectedText) as JwtSecurityToken;

            if (token == null)
            {
                throw new ArgumentOutOfRangeException("protectedText", Properties.Resources.Exception_InvalidJwt);
            }

            ClaimsPrincipal claimsPrincipal = TokenHandler.ValidateToken(protectedText, _validationParameters);
            var claimsIdentity = (ClaimsIdentity)claimsPrincipal.Identity;

            // Fill out the authenticationExtra issued and expires times if the equivalent claims are in the JWT
            var authenticationExtra = new AuthenticationProperties(new Dictionary<string, string>());
            if (claimsIdentity.Claims.Any(c => c.Type == ExpiryClaimName))
            {
                string expiryClaim = (from c in claimsIdentity.Claims where c.Type == ExpiryClaimName select c.Value).Single();
                authenticationExtra.ExpiresUtc = Epoch.AddSeconds(Convert.ToInt64(expiryClaim, CultureInfo.InvariantCulture));
            }

            if (claimsIdentity.Claims.Any(c => c.Type == IssuedAtClaimName))
            {
                string issued = (from c in claimsIdentity.Claims where c.Type == IssuedAtClaimName select c.Value).Single();
                authenticationExtra.IssuedUtc = Epoch.AddSeconds(Convert.ToInt64(issued, CultureInfo.InvariantCulture));
            }

            // Finally, create a new ClaimsIdentity so the auth type is JWT rather than Federated.
            var returnedIdentity = new ClaimsIdentity(claimsIdentity.Claims, "JWT");

            return new AuthenticationTicket(returnedIdentity, authenticationExtra);
        }
    }
}
