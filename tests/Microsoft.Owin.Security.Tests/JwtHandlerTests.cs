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
    public class JwtHandlerTests
    {
        [Fact]
        public void HandlerConstructorShouldThrowWhenAnAllowedAudienceIsNotSpecified()
        {
            Should.Throw<ArgumentNullException>(() => new JwtFormat(null, (IIssuerSecurityTokenProvider)null));
        }

        [Fact]
        public void HandlerConstructorShouldThrowWhenAnAllowedAudienceIsEmpty()
        {
            Should.Throw<ArgumentNullException>(() => new JwtFormat(string.Empty, null));
        }

        [Fact]
        public void HandlerConstructorShouldThrowWhenTheIssuerSecurityTokenProviderIsNull()
        {
            Should.Throw<ArgumentNullException>(() => new JwtFormat("urn:issuer", null));
        }

        [Fact]
        public void HandlerConstructorShouldThrowWhenTheAudiencesEnumerableIsEmpty()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => new JwtFormat(new List<string>(), null));
        }

        [Fact]
        public void HandlerConstructorShouldThrowWhenTheIssuerSecurityTokenProviderEnumerableIsEmpty()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => new JwtFormat(new[] { "urn:issuer" }, new List<IIssuerSecurityTokenProvider>()));
        }

        [Fact]
        public void HandlerConstructorShouldThrowWhenASigningCredentialIsNotSpecified()
        {
            Should.Throw<ArgumentNullException>(() => new JwtFormat(null));
        }

        [Fact]
        public void HandlerConstructorShouldNotThrowWithValidValues()
        {
            var instance = new JwtFormat("http://audience/", new TestIssuerSecurityTokenProvider("urn:issuer"));

            instance.ShouldNotBe(null);
        }

        [Fact]
        public void ProtectShouldThrowArgumentNullExceptionWhenPassedANullAuthenticationTicket()
        {
            var instance = new JwtFormat("http://contoso.com", new TestIssuerSecurityTokenProvider("urn:issuer"));

            Should.Throw<ArgumentNullException>(() => instance.Protect(null));
        }

        [Fact]
        public void ProtectShouldCreateAnAppropriateJwtWithASymmetricSigningKey()
        {
            var instance = new JwtFormat(new TestSigningSecurityTokenProvider());

            var identity = new ClaimsIdentity(
                new[] { new Claim("name", "name") },
                "test",
                "name",
                "role");
            var extra = new AuthenticationProperties();
            extra.Dictionary.Add(JwtFormat.AudiencePropertyKey, "http://fabrikam.com");

            string jwt = instance.Protect(new AuthenticationTicket(identity, extra));

            jwt.ShouldNotBe(null);
        }

        private class TestIssuerSecurityTokenProvider : IIssuerSecurityTokenProvider
        {
            public TestIssuerSecurityTokenProvider(string issuer)
            {
                Issuer = issuer;
            }

            public virtual string Issuer { get; private set; }

            public virtual IEnumerable<SecurityToken> SecurityTokens
            {
                get { throw new NotImplementedException(); }
            }
        }

        private class TestSigningSecurityTokenProvider : TestIssuerSecurityTokenProvider, ISigningCredentialsProvider
        {
            private const string TokenIssuer = "http://contoso.com/";
            private readonly AesManaged _signingAlgorithm = new AesManaged();

            public TestSigningSecurityTokenProvider() : base(TokenIssuer)
            {
            }

            public override string Issuer
            {
                get { return TokenIssuer; }
            }

            public SigningCredentials SigningCredentials
            {
                get
                {
                    return new SigningCredentials(
                        new InMemorySymmetricSecurityKey(_signingAlgorithm.Key),
                        SecurityAlgorithms.HmacSha256Signature,
                        SecurityAlgorithms.Sha256Digest);
                }
            }

            public override IEnumerable<SecurityToken> SecurityTokens
            {
                get { return new List<SecurityToken> { new BinarySecretSecurityToken(_signingAlgorithm.Key) }; }
            }
        }
    }
}
