// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
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
            instance.GetSecurityTokens().Count().ShouldBeGreaterThanOrEqualTo(1);
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
            var instance = new SelfSignedJwtSecureDataHandler(Issuer);
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimsIdentity.DefaultNameClaimType, NameValue) });

            var extra = new AuthenticationExtra { IssuedUtc = DateTime.UtcNow };
            extra.ExpiresUtc = extra.IssuedUtc + new TimeSpan(0, 1, 0, 0);
            extra.Properties.Add(JwtSecureDataHandler.AudiencePropertyKey, Issuer);

            var jwt = instance.Protect(new AuthenticationTicket(identity, extra));

            jwt.ShouldNotBe(null);

            var authenticationTicket = instance.Unprotect(jwt);

            authenticationTicket.ShouldNotBe(null);

            authenticationTicket.Identity.Name.ShouldBe(NameValue);
        }

        [Fact]
        public void KeyShouldRotateOnAfterTheConfiguredTimeSpan()
        {
            const string Issuer = "http://contoso.com/";
            var instance = new SelfSigningJwtProvider(Issuer, new TimeSpan(0, 59, 0)) { SystemClock = new HourIncrementingClock() };
            var firstKey = instance.SigningCredentials;
            var secondKey = instance.SigningCredentials;

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
                var throwaway = instance.SigningCredentials;
// ReSharper restore UnusedVariable
            }
            
            instance.GetSecurityTokens().Count().ShouldBe(5);
        }

        private class HourIncrementingClock : ISystemClock
        {
            private int callCounter;
            private DateTimeOffset intialTime = DateTimeOffset.UtcNow;

            public DateTimeOffset UtcNow
            {
                get
                {
                    intialTime = intialTime + new TimeSpan(callCounter, 0, 0);
                    callCounter++;
                    return intialTime;
                }
            }
        }
    }
}
