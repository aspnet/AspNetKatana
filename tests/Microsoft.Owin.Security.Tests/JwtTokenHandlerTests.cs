// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.ServiceModel.Security.Tokens;

using Microsoft.Owin.Security.Jwt;

using Shouldly;
using Xunit;

namespace Microsoft.Owin.Security.Tests
{

    public class JwtTokenHandlerTests
    {
        [Fact]
        public void ConstructorShouldThrowWhenASecurityTokenProviderIsNotSpecified()
        {
            Should.Throw<ArgumentNullException>(() => new JwtTokenHandler(null));            
        }

        [Fact]
        public void ConstructorShouldNotThrowWithValidValues()
        {
            var instance = new JwtTokenHandler(new SecurityTokenProvider());

            instance.ShouldNotBe(null);
        }

        [Fact]
        public void ProtectShouldThrowArgumentNullExceptionWhenPassedANullAuthenticationTicket()
        {
            var instance = new JwtTokenHandler(new SecurityTokenProvider());

            Should.Throw<ArgumentNullException>(() => instance.Protect(null)); 
        }

        [Fact]
        public void ProtectShouldThrowNotSupportedExceptionIfTheTokenProviderIsNotASigningTokenProvider()
        {
            var instance = new JwtTokenHandler(new SecurityTokenProvider());

            var identity = new ClaimsIdentity();
            var extra = new AuthenticationExtra();

            Should.Throw<NotSupportedException>(() => instance.Protect(new AuthenticationTicket(identity, extra)));             
        }

        [Fact]
        public void ProtectShouldCreateAnAppropriateJwtWithASymmetricSigningKey()
        {
            var instance = new JwtTokenHandler(new SigningSecurityTokenProvider());

            var identity = new ClaimsIdentity(
                new[] { new Claim("name", "name") },
                "test",
                "name",
                "role");
            var extra = new AuthenticationExtra();
            extra.Properties.Add(JwtTokenHandler.AudiencePropertyKey, "http://fabrikam.com");

            var jwt = instance.Protect(new AuthenticationTicket(identity, extra));

            jwt.ShouldNotBe(null);
        }

        [Fact]
        public void ProtectThenUnprotectWithTheSameIdentityShouldResultInTheSameClaims()
        {
            var instance = new JwtTokenHandler(new SigningSecurityTokenProvider());

            var identity = new ClaimsIdentity(
                new[] { new Claim("name", "name") },
                "test",
                "name",
                "role");
            var extra = new AuthenticationExtra();
            extra.Properties.Add(JwtTokenHandler.AudiencePropertyKey, "http://fabrikam.com");

            var jwt = instance.Protect(new AuthenticationTicket(identity, extra));

            jwt.ShouldNotBe(null);

            var authenticationTicket = instance.Unprotect(jwt);


        }

        private class SecurityTokenProvider : ISecurityTokenProvider
        {
            public SecurityToken GetTokenForIdentifier(string identifier)
            {
                throw new NotImplementedException();
            }

            public SecurityToken GetSigningTokenForIssuer(string issuer)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<SecurityToken> GetSigningTokens()
            {
                throw new NotImplementedException();
            }

            public JwtValidationParameters ValidationParameters
            {
                get { throw new NotImplementedException(); }
            }

            public IEnumerable<string> ExpectedAudiences
            {
                get { throw new NotImplementedException(); }
            }
        }

        private class SigningSecurityTokenProvider : SecurityTokenProvider, ISigningSecurityTokenProvider
        {
            private readonly AesManaged signingAlgorithm = new AesManaged();

            public string Issuer
            {
                get
                {
                    return "http://contoso.com/";
                }
            }

            public SigningCredentials SigningCredentials
            {
                get 
                {
                    return new SigningCredentials(
                        new InMemorySymmetricSecurityKey(signingAlgorithm.Key), 
                        SecurityAlgorithms.HmacSha256Signature, 
                        SecurityAlgorithms.Sha256Digest);
                }
            }

            public JwtValidationParameters ValidationParameters
            {
                get { return JwtValidationParameters.Issuer; }
            }


            public IEnumerable<SecurityToken> GetSigningTokens()
            {
                return new List<SecurityToken> { new BinarySecretSecurityToken(signingAlgorithm.Key) };
            }

            public SecurityToken GetSigningTokenForIssuer(string issuer)
            {
                if (issuer == Issuer)
                {
                    return new BinarySecretSecurityToken(signingAlgorithm.Key);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("issuer");
                }

            }

            public IEnumerable<string> ExpectedAudiences
            {
                get
                {
                    return new [] { "http://fabrikam.com" };
                }
            }
        }

    }
}
