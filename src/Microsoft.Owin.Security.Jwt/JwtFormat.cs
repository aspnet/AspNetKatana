﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Owin.Security.Jwt
{
    /// <summary>
    /// Signs and validates JSON Web Tokens.
    /// </summary>
    public class JwtFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly TokenValidationParameters _validationParameters;
        private readonly IEnumerable<IIssuerSecurityKeyProvider> _issuerCredentialProviders;
        private JwtSecurityTokenHandler _tokenHandler;

        /// <summary>
        /// Creates a new JwtFormat with TokenHandler and UseTokenLifetime enabled by default.
        /// </summary>
        protected JwtFormat()
        {
            TokenHandler = new JwtSecurityTokenHandler();
            UseTokenLifetime = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtFormat"/> class.
        /// </summary>
        /// <param name="allowedAudience">The allowed audience for JWTs.</param>
        /// <param name="issuerCredentialProvider">The issuer credential provider.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="issuerCredentialProvider"/> is null.</exception>
        public JwtFormat(string allowedAudience, IIssuerSecurityKeyProvider issuerCredentialProvider)
            : this()
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
                AuthenticationType = "JWT",
            };
            _issuerCredentialProviders = new[] { issuerCredentialProvider };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtFormat"/> class.
        /// </summary>
        /// <param name="allowedAudiences">The allowed audience for JWTs.</param>
        /// <param name="issuerCredentialProviders">The issuer credential provider.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="issuerCredentialProviders"/> is null.</exception>
        public JwtFormat(IEnumerable<string> allowedAudiences, IEnumerable<IIssuerSecurityKeyProvider> issuerCredentialProviders)
            : this()
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
            var credentialProviders = new List<IIssuerSecurityKeyProvider>(issuerCredentialProviders);
            if (!credentialProviders.Any())
            {
                throw new ArgumentOutOfRangeException("issuerCredentialProviders", Properties.Resources.Exception_IssuerCredentialProvidersMustBeSpecified);
            }

            _validationParameters = new TokenValidationParameters()
            {
                ValidAudiences = audiences,
                AuthenticationType = "JWT",
            };
            _issuerCredentialProviders = issuerCredentialProviders;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtFormat"/> class.
        /// </summary>
        /// <param name="validationParameters"> <see cref="TokenValidationParameters"/> used to determine if a token is valid.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="validationParameters"/> is null.</exception>
        public JwtFormat(TokenValidationParameters validationParameters)
            : this()
        {
            if (validationParameters == null)
            {
                throw new ArgumentNullException("validationParameters");
            }

            _validationParameters = validationParameters;

            if (string.IsNullOrWhiteSpace(_validationParameters.AuthenticationType))
            {
                _validationParameters.AuthenticationType = "JWT";
            }
        }

        public JwtFormat(TokenValidationParameters validationParameters, IIssuerSecurityKeyProvider issuerCredentialProvider)
            : this(validationParameters)
        {
            if (issuerCredentialProvider == null)
            {
                throw new ArgumentNullException("issuerCredentialProvider");
            }

            _issuerCredentialProviders = new[] { issuerCredentialProvider };
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
        public JwtSecurityTokenHandler TokenHandler
        {
            get { return _tokenHandler; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _tokenHandler = value;
            }
        }

        /// <summary>
        /// Indicates that the authentication session lifetime (e.g. cookies) should match that of the authentication token.
        /// If the token does not provide lifetime information then normal session lifetimes will be used.
        /// This is enabled by default.
        /// </summary>
        public bool UseTokenLifetime { get; set; }

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

            var token = TokenHandler.ReadToken(protectedText) as JwtSecurityToken;

            if (token == null)
            {
                throw new ArgumentOutOfRangeException("protectedText", Properties.Resources.Exception_InvalidJwt);
            }

            TokenValidationParameters validationParameters = _validationParameters;
            if (_issuerCredentialProviders != null)
            {
                // Lazy augment with issuers and tokens. Note these may be refreshed periodically.
                validationParameters = validationParameters.Clone();

                IEnumerable<string> issuers = _issuerCredentialProviders.Select(provider => provider.Issuer);
                if (validationParameters.ValidIssuers == null)
                {
                    validationParameters.ValidIssuers = issuers;
                }
                else
                {
                    validationParameters.ValidIssuers = validationParameters.ValidIssuers.Concat(issuers);
                }

                var tokens = _issuerCredentialProviders.Select(provider => provider.SecurityKeys).Aggregate((left, right) => left.Concat(right));
                Collection<SecurityKey> keys = GetKeys(tokens);

                if (validationParameters.IssuerSigningKeys == null)
                {
                    validationParameters.IssuerSigningKeys = keys;
                }
                else
                {
                    validationParameters.IssuerSigningKeys = validationParameters.IssuerSigningKeys.Concat(keys);
                }
            }

            Microsoft.IdentityModel.Tokens.SecurityToken validatedToken;
            ClaimsPrincipal claimsPrincipal = TokenHandler.ValidateToken(protectedText, validationParameters, out validatedToken);
            var claimsIdentity = (ClaimsIdentity)claimsPrincipal.Identity;

            // Fill out the authenticationProperties issued and expires times if the equivalent claims are in the JWT
            var authenticationProperties = new AuthenticationProperties();

            if (UseTokenLifetime)
            {
                // Override any session persistence to match the token lifetime.
                DateTime issued = validatedToken.ValidFrom;
                if (issued != DateTime.MinValue)
                {
                    authenticationProperties.IssuedUtc = issued.ToUniversalTime();
                }
                DateTime expires = validatedToken.ValidTo;
                if (expires != DateTime.MinValue)
                {
                    authenticationProperties.ExpiresUtc = expires.ToUniversalTime();
                }

                authenticationProperties.AllowRefresh = false;
            }

            return new AuthenticationTicket(claimsIdentity, authenticationProperties);
        }

        private static Collection<SecurityKey> GetKeys(IEnumerable<SecurityKey> keys)
        {
            var keyCollection = new Collection<SecurityKey>();

            foreach (var item in keys)
            {
                keyCollection.Add(item);
            }
                
            return keyCollection;
        }
    }
}
