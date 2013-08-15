// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security.Jwt;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Security.Tests
{
    public class SelfSigningTokenProviderTests
    {
        [Fact]
        public void ConstructorShouldThrowWhenANullIssuerIsProvided()
        {
            Should.Throw<ArgumentNullException>(() => new SelfSigningJwtProvider(null));
        }

        [Fact]
        public void ShouldReturnAKeyAfterInitialization()
        {
            const string Issuer = "http://contoso.com/";
            var instance = new SelfSigningJwtProvider(Issuer);

            instance.ShouldNotBe(null);
            instance.SigningCredentials.ShouldNotBe(null);
            instance.SecurityTokens.Count().ShouldBeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public void IssuerShouldBeSetCorrectly()
        {
            const string Issuer = "http://contoso.com/";
            var instance = new SelfSigningJwtProvider(Issuer);

            instance.Issuer.ShouldBe(Issuer);
        }

        [Fact]
        public void SelfSignedHandlerProtectThenUnprotectShouldResultInTheSameClaims()
        {
            const string Issuer = "http://contoso.com/";
            const string NameValue = "NameValue";
            var instance = new SelfSignedJwtFormat(Issuer);
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimsIdentity.DefaultNameClaimType, NameValue) });

            var extra = new AuthenticationProperties { IssuedUtc = DateTime.UtcNow };
            extra.ExpiresUtc = extra.IssuedUtc + new TimeSpan(0, 1, 0, 0);
            extra.Dictionary.Add(JwtFormat.AudiencePropertyKey, Issuer);

            string jwt = instance.Protect(new AuthenticationTicket(identity, extra));

            jwt.ShouldNotBe(null);

            AuthenticationTicket authenticationTicket = instance.Unprotect(jwt);

            authenticationTicket.ShouldNotBe(null);

            authenticationTicket.Identity.Name.ShouldBe(NameValue);
        }

        [Fact]
        public void IssuedKeysShouldHaveAKeyIdentifierClause()
        {
            const string Issuer = "http://contoso.com/";
            var instance = new SelfSigningJwtProvider(Issuer);

            SigningCredentials key = instance.SigningCredentials;
            key.SigningKeyIdentifier.Count.ShouldBe(1);

            SecurityKeyIdentifierClause keyIdentifier = key.SigningKeyIdentifier[0];
            keyIdentifier.ClauseType.ShouldBe("NamedKeySecurityKeyIdentifierClause");

            var namedKeyIdentifierClause = keyIdentifier as NamedKeySecurityKeyIdentifierClause;
            namedKeyIdentifierClause.ShouldNotBe(null);
// ReSharper disable PossibleNullReferenceException
            namedKeyIdentifierClause.KeyIdentifier.ShouldNotBeEmpty();
// ReSharper restore PossibleNullReferenceException
        }

        [Fact]
        public void IssuedKeysShouldHaveDifferentKeyIdentifierClause()
        {
            const string Issuer = "http://contoso.com/";
            var instance = new SelfSigningJwtProvider(Issuer, new TimeSpan(0, 59, 0)) { SystemClock = new HourIncrementingClock() };

            SigningCredentials firstKey = instance.SigningCredentials;
            string firstKeyIdentifier = ((NamedKeySecurityKeyIdentifierClause)(firstKey.SigningKeyIdentifier[0])).KeyIdentifier;
            SigningCredentials secondKey = instance.SigningCredentials;
            string secondKeyIdentifier = ((NamedKeySecurityKeyIdentifierClause)(secondKey.SigningKeyIdentifier[0])).KeyIdentifier;
            secondKeyIdentifier.ShouldNotBeSameAs(firstKeyIdentifier);
        }

        [Fact]
        public void KeyShouldRotateOnAfterTheConfiguredTimeSpan()
        {
            const string Issuer = "http://contoso.com/";
            var instance = new SelfSigningJwtProvider(Issuer, new TimeSpan(0, 59, 0)) { SystemClock = new HourIncrementingClock() };
            SigningCredentials firstKey = instance.SigningCredentials;
            SigningCredentials secondKey = instance.SigningCredentials;

            firstKey.ShouldNotBe(secondKey);
        }

        [Fact]
        public void TheMaximumNumberOfRotatedKeysShouldBeLimitedToFive()
        {
            const string Issuer = "http://contoso.com/";
            var instance = new SelfSigningJwtProvider(Issuer, new TimeSpan(0, 59, 0)) { SystemClock = new HourIncrementingClock() };

            for (int i = 0; i < 10; i++)
            {
// ReSharper disable UnusedVariable
                SigningCredentials throwaway = instance.SigningCredentials;
// ReSharper restore UnusedVariable
            }

            instance.SecurityTokens.Count().ShouldBe(5);
        }

        [Fact]
        public void KeysShouldNotRotateWithAStaticClock()
        {
            const string Issuer = "http://contoso.com/";
            var instance = new SelfSigningJwtProvider(Issuer, new TimeSpan(0, 59, 0)) { SystemClock = new StaticClock() };

            for (int i = 0; i < 10; i++)
            {
                // ReSharper disable UnusedVariable
                SigningCredentials throwaway = instance.SigningCredentials;
                // ReSharper restore UnusedVariable
            }

            instance.SecurityTokens.Count().ShouldBe(1);
        }

        [Fact]
        public void SelfSignedHandlerProtectThenUnprotectShouldResultInTheSameClaimsAfterAKeyRotation()
        {
            const string Issuer = "http://contoso.com/";
            const string NameValue = "NameValue";
            var provider = new SelfSigningJwtProvider(Issuer, new TimeSpan(0, 59, 0)) { SystemClock = new HourIncrementingClock() };

            var instance = new JwtFormat(provider);

            var identity = new ClaimsIdentity(new[] { new Claim(ClaimsIdentity.DefaultNameClaimType, NameValue) });

            var extra = new AuthenticationProperties { IssuedUtc = DateTime.UtcNow };
            extra.ExpiresUtc = extra.IssuedUtc + new TimeSpan(0, 1, 0, 0);
            extra.Dictionary.Add(JwtFormat.AudiencePropertyKey, Issuer);

            string jwt = instance.Protect(new AuthenticationTicket(identity, extra));

            jwt.ShouldNotBe(null);

            // Now we have the lot. Let's rotate the keys.
            // ReSharper disable UnusedVariable
            SigningCredentials throwaway = provider.SigningCredentials;
            // ReSharper restore UnusedVariable

            AuthenticationTicket authenticationTicket = instance.Unprotect(jwt);

            authenticationTicket.ShouldNotBe(null);

            authenticationTicket.Identity.Name.ShouldBe(NameValue);
        }

        [Fact]
        public void SelfSignedHandlerProtectThenUnprotectShouldFailOnceTheKeyHasDroppedOutOfRotation()
        {
            const string Issuer = "http://contoso.com/";
            const string NameValue = "NameValue";
            var provider = new SelfSigningJwtProvider(Issuer, new TimeSpan(0, 59, 0)) { SystemClock = new HourIncrementingClock() };

            var instance = new JwtFormat(provider);

            var identity = new ClaimsIdentity(new[] { new Claim(ClaimsIdentity.DefaultNameClaimType, NameValue) });

            var extra = new AuthenticationProperties { IssuedUtc = DateTime.UtcNow };
            extra.ExpiresUtc = extra.IssuedUtc + new TimeSpan(0, 1, 0, 0);
            extra.Dictionary.Add(JwtFormat.AudiencePropertyKey, Issuer);

            string jwt = instance.Protect(new AuthenticationTicket(identity, extra));

            jwt.ShouldNotBe(null);

            // Now we have the lot. Let's rotate the keys until the key used to sign drops away.
            // ReSharper disable UnusedVariable
            for (int i = 0; i < 10; i++)
            {
                SigningCredentials throwaway = provider.SigningCredentials;
            }
            // ReSharper restore UnusedVariable

            Should.Throw<SecurityTokenValidationException>(() => instance.Unprotect(jwt));
        }

        private class HourIncrementingClock : ISystemClock
        {
            private int _callCounter;
            private DateTimeOffset _initialTime = DateTimeOffset.UtcNow;

            public DateTimeOffset UtcNow
            {
                get
                {
                    _initialTime = _initialTime + new TimeSpan(_callCounter, 0, 0);
                    _callCounter++;
                    return _initialTime;
                }
            }
        }

        private class StaticClock : ISystemClock
        {
            private readonly DateTimeOffset _time = DateTimeOffset.UtcNow;

            public DateTimeOffset UtcNow
            {
                get { return _time; }
            }
        }
    }
}
