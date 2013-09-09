// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
    public class JwtFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private const string IssuedAtClaimName = "iat";

        private const string ExpiryClaimName = "exp";

        private const string JwtIdClaimName = "jti";

        private static DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly List<string> _allowedAudiences = new List<string>();

        private readonly Dictionary<string, IIssuerSecurityTokenProvider> _issuerCredentialProviders = new Dictionary<string, IIssuerSecurityTokenProvider>();

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

            _allowedAudiences.Add(allowedAudience);

            _issuerCredentialProviders.Add(issuerCredentialProvider.Issuer, issuerCredentialProvider);

            ValidateIssuer = true;
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

            ValidateIssuer = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether JWT issuers should be validated.
        /// </summary>
        /// <value>
        /// true if the issuer should be validate; otherwise, false.
        /// </value>
        public bool ValidateIssuer { get; set; }

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

            var handler = new JwtSecurityTokenHandler
            {
                CertificateValidator = X509CertificateValidator.None
            };

            var token = handler.ReadToken(protectedText) as JwtSecurityToken;

            if (token == null)
            {
                throw new ArgumentOutOfRangeException("protectedText", Properties.Resources.Exception_InvalidJwt);
            }

            var validationParameters = new TokenValidationParameters { AllowedAudiences = _allowedAudiences, ValidateIssuer = ValidateIssuer };

            if (ValidateIssuer)
            {
                if (string.IsNullOrWhiteSpace(token.Issuer))
                {
                    throw new ArgumentOutOfRangeException("protectedText", Properties.Resources.Exception_CannotValidateIssuer);
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

            ClaimsPrincipal claimsPrincipal = handler.ValidateToken(protectedText, validationParameters);
            var claimsIdentity = (ClaimsIdentity)claimsPrincipal.Identity;

            // Fill out the authenticationExtra issued and expires times if the equivalent claims are in the JWT
            var authenticationExtra = new AuthenticationProperties(new Dictionary<string, string>());
            if (claimsIdentity.Claims.Any(c => c.Type == ExpiryClaimName))
            {
                string expiryClaim = (from c in claimsIdentity.Claims where c.Type == ExpiryClaimName select c.Value).Single();
                authenticationExtra.ExpiresUtc = _epoch.AddSeconds(Convert.ToInt64(expiryClaim, CultureInfo.InvariantCulture));
            }

            if (claimsIdentity.Claims.Any(c => c.Type == IssuedAtClaimName))
            {
                string issued = (from c in claimsIdentity.Claims where c.Type == IssuedAtClaimName select c.Value).Single();
                authenticationExtra.IssuedUtc = _epoch.AddSeconds(Convert.ToInt64(issued, CultureInfo.InvariantCulture));
            }

            // Finally, create a new ClaimsIdentity so the auth type is JWT rather than Federated.
            var returnedIdentity = new ClaimsIdentity(claimsIdentity.Claims, "JWT");

            return new AuthenticationTicket(returnedIdentity, authenticationExtra);
        }
    }
}
