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
            Should.Throw<ArgumentNullException>(() => new JwtFormat((string)null, (IIssuerSecurityTokenProvider)null));
            Should.Throw<ArgumentNullException>(() => new JwtFormat((TokenValidationParameters)null, (IIssuerSecurityTokenProvider)null));
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
        public void HandlerConstructorShouldNotThrowWithValidValues()
        {
            var instance = new JwtFormat("http://audience/", new TestIssuerSecurityTokenProvider("urn:issuer"));

            instance.ShouldNotBe(null);
        }

        [Fact]
        public void ProtectShouldThrowNotImplementedException()
        {
            var instance = new JwtFormat("http://contoso.com", new TestIssuerSecurityTokenProvider("urn:issuer"));

            Should.Throw<NotSupportedException>(() => instance.Protect(null));
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
                get { return new SecurityToken[0]; }
            }
        }
    }
}
