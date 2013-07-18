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
            Should.Throw<ArgumentNullException>(() => new SelfSigningTokenProvider(null));
        }

        [Fact]
        public void ShouldReturnAKeyAfterInitialization()
        {
            const string Issuer = "http://contoso.com/";
            var instance = new SelfSigningTokenProvider(Issuer);
            
            instance.GetSigningTokensForIssuer(Issuer).ShouldNotBe(null);
            instance.GetSigningTokensForIssuer(Issuer).Count().ShouldBeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public void TheIssuerShouldAlsoBeTheSoleAudience()
        {
            const string Issuer = "http://contoso.com/";
            var instance = new SelfSigningTokenProvider(Issuer);

            instance.ExpectedAudiences.Count().ShouldBe(1);
            instance.ExpectedAudiences.ElementAt(0).ShouldBe(Issuer);
        }

        [Fact]
        public void ProtectThenUnprotectShouldResultInTheSameClaims()
        {
            const string Issuer = "http://contoso.com/";
            const string NameValue = "NameValue";
            var instance = new JwtTokenHandler(new SelfSigningTokenProvider(Issuer));
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimsIdentity.DefaultNameClaimType, NameValue) });

            var extra = new AuthenticationExtra { IssuedUtc = DateTime.UtcNow };
            extra.ExpiresUtc = extra.IssuedUtc + new TimeSpan(0, 1, 0, 0);
            extra.Properties.Add(JwtTokenHandler.AudiencePropertyKey, Issuer);

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
            var instance = new SelfSigningTokenProvider(Issuer, new TimeSpan(0, 59, 0)) { SystemClock = new HourIncrementingClock() };
            var firstKey = instance.SigningCredentials;
            var secondKey = instance.SigningCredentials;

            firstKey.ShouldNotBe(secondKey);
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
