// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using FunctionalTests.Facts.Security.Common;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Xunit;

namespace FunctionalTests.Facts.Security.Jwt
{
    public class X509CertificateTokenVerification
    {
        [Fact, Trait("FunctionalTests", "Security")]
        public void Security_X509CertificateTokenVerificationFact()
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

            var signingCertificate = GetACertificateWithPrivateKey();
            var signingCredentials = new X509SigningCredentials(signingCertificate);
            var tokenValidationParameters = new TokenValidationParameters() { ValidAudience = issuer, SaveSigninToken = true, AuthenticationType = sentIdentity.AuthenticationType };
            var formatter = new JwtFormat(tokenValidationParameters, new X509CertificateSecurityTokenProvider(issuer, signingCertificate));
            formatter.TokenHandler = new JwtSecurityTokenHandler();

            var protectedtext = SecurityUtils.CreateJwtToken(sentTicket, issuer, signingCredentials);

            //Receive part
            var receivedTicket = formatter.Unprotect(protectedtext);

            var receivedClaims = receivedTicket.Identity.Claims;
            Assert.Equal<string>("CustomJwt", receivedTicket.Identity.AuthenticationType);
            Assert.Equal<string>(ClaimsIdentity.DefaultNameClaimType, receivedTicket.Identity.NameClaimType);
            Assert.Equal<string>(ClaimsIdentity.DefaultRoleClaimType, receivedTicket.Identity.RoleClaimType);
            Assert.NotNull(receivedTicket.Identity.BootstrapContext);
            Assert.NotNull(((BootstrapContext)receivedTicket.Identity.BootstrapContext).Token);
            Assert.Equal<string>(issuer, receivedClaims.Where<Claim>(claim => claim.Type == "iss").FirstOrDefault().Value);
            Assert.Equal<string>(issuer, receivedClaims.Where<Claim>(claim => claim.Type == "aud").FirstOrDefault().Value);
            Assert.NotEmpty(receivedClaims.Where<Claim>(claim => claim.Type == "exp").FirstOrDefault().Value);

            for (int i = 0; i < 5; i++)
            {
                sentIdentity.AddClaim(new Claim("ClaimId" + i.ToString(), i.ToString()));
                Assert.Equal<string>(i.ToString(), receivedClaims.Where<Claim>(claim => claim.Type == "ClaimId" + i.ToString()).FirstOrDefault().Value);
            }

            Assert.Equal<string>("TestUser", receivedClaims.Where<Claim>(claim => claim.Type == ClaimsIdentity.DefaultNameClaimType).FirstOrDefault().Value);
            Assert.Equal<string>("Administrator", receivedClaims.Where<Claim>(claim => claim.Type == ClaimsIdentity.DefaultRoleClaimType).FirstOrDefault().Value);
            Assert.NotEmpty(receivedClaims.Where<Claim>(claim => claim.Type == "iat").FirstOrDefault().Value);
            Assert.NotEmpty(receivedClaims.Where<Claim>(claim => claim.Type == "jti").FirstOrDefault().Value);
        }

        private static X509Certificate2 GetACertificateWithPrivateKey()
        {
            var certificate = GetACertificateWithPrivateKeyInStore(StoreName.My, StoreLocation.LocalMachine);
            if (certificate == null)
            {
                certificate = GetACertificateWithPrivateKeyInStore(StoreName.My, StoreLocation.CurrentUser);
            }

            return certificate;
        }

        private static X509Certificate2 GetACertificateWithPrivateKeyInStore(StoreName storeName, StoreLocation storeLocation)
        {
            Trace.WriteLine(string.Format("Looking for certificates in store : {0}, store location : {1}", storeName, storeLocation));

            var certificateStore = new X509Store(storeName, storeLocation);
            certificateStore.Open(OpenFlags.ReadOnly);
            foreach (var certificate in certificateStore.Certificates)
            {
                if (certificate.HasPrivateKey && certificate.PublicKey.Key.KeySize == 2048)
                {
                    try
                    {
                        var key = certificate.PrivateKey;
                        Trace.WriteLine("Found a suitable certificate with a private key");
                        Trace.WriteLine(string.Format("Certificate issuer : {0}, Subject Name : {1}", certificate.Issuer, certificate.Subject));
                        return certificate;
                    }
                    catch (Exception)
                    {
                        Trace.WriteLine("Ignoring a Cryptography Next generation (CNG) cert");
                    }
                }
            }

            return null;
        }
    }
}