// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
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
            const string Audience = "http://fabrikam.com";
            const string NameClaim = "NameClaim";
            const string NameValue = "NameValue";
            const string AuthenticationType = "Test";
            var instance = new JwtTokenHandler(new SigningSecurityTokenProvider());

            var identity = new ClaimsIdentity(
                new[] { new Claim(NameClaim, NameValue) },
                AuthenticationType,
                NameClaim,
                "role");
            var extra = new AuthenticationExtra { IssuedUtc = DateTime.UtcNow };
            extra.ExpiresUtc = extra.IssuedUtc + new TimeSpan(0, 1, 0, 0);
            extra.Properties.Add(JwtTokenHandler.AudiencePropertyKey, Audience);

            var jwt = instance.Protect(new AuthenticationTicket(identity, extra));

            jwt.ShouldNotBe(null);

            var authenticationTicket = instance.Unprotect(jwt);

            authenticationTicket.ShouldNotBe(null);
                        
            (from c in authenticationTicket.Identity.Claims where c.Type == NameClaim select c.Value).Single().ShouldBe(NameValue);
            (from c in authenticationTicket.Identity.Claims where c.Type == "aud" select c.Value).Single().ShouldBe(Audience);

            authenticationTicket.Extra.IssuedUtc.ShouldBe(extra.IssuedUtc);
            authenticationTicket.Extra.ExpiresUtc.ShouldBe(extra.ExpiresUtc);
        }

        private class SecurityTokenProvider : ISecurityTokenProvider
        {
            public virtual bool ValidateIssuer
            {
                get { return false; }
            }

            public virtual IEnumerable<string> ExpectedAudiences
            {
                get { throw new NotImplementedException(); }
            }
            
            public virtual SecurityToken GetSigningTokenForKeyIdentifier(string identifier)
            {
                throw new NotImplementedException();
            }

            public virtual SecurityToken GetSigningTokenForKeyIdentifier(string issuer, string identifier)
            {
                throw new NotImplementedException();
            }

            public virtual IEnumerable<SecurityToken> GetSigningTokensForIssuer(string issuer)
            {
                throw new NotImplementedException();
            }

            public virtual IEnumerable<SecurityToken> GetSigningTokens()
            {
                throw new NotImplementedException();
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

            public override bool ValidateIssuer
            {
                get { return true; }
            }

            public override IEnumerable<string> ExpectedAudiences
            {
                get
                {
                    return new[] { "http://fabrikam.com" };
                }
            }

            public override IEnumerable<SecurityToken> GetSigningTokens()
            {
                return new List<SecurityToken> { new BinarySecretSecurityToken(signingAlgorithm.Key) };
            }

            public override IEnumerable<SecurityToken> GetSigningTokensForIssuer(string issuer)
            {
                if (issuer == Issuer)
                {
                    return GetSigningTokens();
                }
                
                throw new ArgumentOutOfRangeException("issuer");
            }
        }
    }
}
