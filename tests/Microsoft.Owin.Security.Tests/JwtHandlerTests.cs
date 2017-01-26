// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
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
            Should.Throw<ArgumentNullException>(() => new JwtFormat((string)null, (IIssuerSecurityKeyProvider)null));
            Should.Throw<ArgumentNullException>(() => new JwtFormat((TokenValidationParameters)null, (IIssuerSecurityKeyProvider)null));
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
            Should.Throw<ArgumentOutOfRangeException>(() => new JwtFormat(new[] { "urn:issuer" }, new List<IIssuerSecurityKeyProvider>()));
        }

        [Fact]
        public void HandlerConstructorShouldNotThrowWithValidValues()
        {
            var instance = new JwtFormat("http://audience/", new TestIssuerSecurityKeyProvider("urn:issuer"));

            instance.ShouldNotBe(null);
        }

        [Fact]
        public void ProtectShouldThrowNotImplementedException()
        {
            var instance = new JwtFormat("http://contoso.com", new TestIssuerSecurityKeyProvider("urn:issuer"));

            Should.Throw<NotSupportedException>(() => instance.Protect(null));
        }

        private class TestIssuerSecurityKeyProvider : IIssuerSecurityKeyProvider
        {
            public TestIssuerSecurityKeyProvider(string issuer)
            {
                Issuer = issuer;
            }

            public virtual string Issuer { get; private set; }

            public virtual IEnumerable<SecurityKey> SecurityKeys
            {
                get { return new SecurityKey[0]; }
            }
        }
    }
}
