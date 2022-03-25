// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using FunctionalTests.Facts.Security.Common;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Xunit;

namespace FunctionalTests.Facts.Security.Jwt
{
    public class SymmetricKeyTokenVerification
    {
        [Fact, Trait("FunctionalTests", "Security")]
        public void Security_SymmetricKeyTokenVerificationFact()
        {
            var issuer = "http://katanatesting.com/";
            var sentIdentity = new ClaimsIdentity("CustomJwt", "MyNameClaimType", "MyRoleClaimType");
            sentIdentity.AddClaims(new Claim[] { new Claim("MyNameClaimType", "TestUser"), new Claim("MyRoleClaimType", "Administrator") });
            for (int i = 0; i < 5; i++)
            {
                sentIdentity.AddClaim(new Claim("ClaimId" + i.ToString(), i.ToString()));
            }

            var authProperties = new AuthenticationProperties();
            var sentTicket = new AuthenticationTicket(sentIdentity, authProperties);

            var signingAlgorithm = new AesManaged();
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(signingAlgorithm.Key), SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest);
            var tokenValidationParameters = new TokenValidationParameters() { ValidAudience = issuer, SaveSigninToken = true, AuthenticationType = sentIdentity.AuthenticationType };
            var formatter = new JwtFormat(tokenValidationParameters, new SymmetricKeyIssuerSecurityKeyProvider(issuer, signingAlgorithm.Key));
            formatter.TokenHandler = new JwtSecurityTokenHandler();

            var protectedtext = SecurityUtils.CreateJwtToken(sentTicket, issuer, signingCredentials);

            //Receive part
            var receivedTicket = formatter.Unprotect(protectedtext);

            var receivedClaims = receivedTicket.Identity.Claims;
            Assert.Equal("CustomJwt", receivedTicket.Identity.AuthenticationType);
            Assert.Equal(ClaimsIdentity.DefaultNameClaimType, receivedTicket.Identity.NameClaimType);
            Assert.Equal(ClaimsIdentity.DefaultRoleClaimType, receivedTicket.Identity.RoleClaimType);
            Assert.NotNull(receivedTicket.Identity.BootstrapContext);
            Assert.NotNull((receivedTicket.Identity.BootstrapContext) as string);
            Assert.Equal(issuer, receivedClaims.Where<Claim>(claim => claim.Type == "iss").FirstOrDefault().Value);
            Assert.Equal(issuer, receivedClaims.Where<Claim>(claim => claim.Type == "aud").FirstOrDefault().Value);
            Assert.NotEmpty(receivedClaims.Where<Claim>(claim => claim.Type == "exp").FirstOrDefault().Value);

            for (int i = 0; i < 5; i++)
            {
                sentIdentity.AddClaim(new Claim("ClaimId" + i.ToString(), i.ToString()));
                Assert.Equal(i.ToString(), receivedClaims.Where<Claim>(claim => claim.Type == "ClaimId" + i.ToString()).FirstOrDefault().Value);
            }

            Assert.Equal("TestUser", receivedClaims.Where<Claim>(claim => claim.Type == ClaimsIdentity.DefaultNameClaimType).FirstOrDefault().Value);
            Assert.Equal("Administrator", receivedClaims.Where<Claim>(claim => claim.Type == ClaimsIdentity.DefaultRoleClaimType).FirstOrDefault().Value);
            Assert.NotEmpty(receivedClaims.Where<Claim>(claim => claim.Type == "iat").FirstOrDefault().Value);
            Assert.NotEmpty(receivedClaims.Where<Claim>(claim => claim.Type == "jti").FirstOrDefault().Value);
        }
    }
}