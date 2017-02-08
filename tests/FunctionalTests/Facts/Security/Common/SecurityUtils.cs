// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using Microsoft.Owin.Security;

namespace FunctionalTests.Facts.Security.Common
{
    public class SecurityUtils
    {
        public static string CreateJwtToken(AuthenticationTicket data, string issuer, SigningCredentials signingCredentials)
        {
            string audience = issuer;

            // As JWT doesn't have a mechanism of passing metadata about what claim should be the name/subject the JWT handler
            // users the default Name claim type. If the identity has another claim type as the name type we need to 
            // switch it to the DefaultNameClaimType.
            var identity = new ClaimsIdentity(data.Identity);
            if (identity.NameClaimType != ClaimsIdentity.DefaultNameClaimType && !string.IsNullOrWhiteSpace(identity.Name))
            {
                identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, identity.Name));
                identity.RemoveClaim(identity.Claims.First(c => c.Type == identity.NameClaimType));
            }

            // And now do the same for roles.
            List<Claim> roleClaims = identity.Claims.Where(c => c.Type == identity.RoleClaimType).ToList();
            if (identity.RoleClaimType != ClaimsIdentity.DefaultRoleClaimType && roleClaims.Any())
            {
                foreach (var roleClaim in roleClaims)
                {
                    identity.RemoveClaim(roleClaim);
                    identity.AddClaim(new Claim(ClaimsIdentity.DefaultRoleClaimType, roleClaim.Value, roleClaim.ValueType, roleClaim.Issuer, roleClaim.OriginalIssuer));
                }
            }

            identity.AddClaims(new[]
            {
                new Claim("iat", GetEpocTimeStamp()),
                new Claim("jti", Guid.NewGuid().ToString("N"))
            });

            Lifetime lifetime = new Lifetime(null, null);
            if (data.Properties.IssuedUtc != null || data.Properties.ExpiresUtc != null)
            {
                lifetime = new Lifetime(data.Properties.IssuedUtc != null ? (DateTime?)((DateTimeOffset)data.Properties.IssuedUtc).UtcDateTime : null, data.Properties.ExpiresUtc != null ? (DateTime?)((DateTimeOffset)data.Properties.ExpiresUtc).UtcDateTime : null);
            }

            var handler = new JwtSecurityTokenHandler();
            return handler.CreateToken(issuer, audience, identity, lifetime.Created, lifetime.Expires, signingCredentials).RawData;
        }

        private static string GetEpocTimeStamp()
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan secondsSinceUnixEpocStart = DateTime.UtcNow - epoch;
            return Convert.ToInt64(secondsSinceUnixEpocStart.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }
    }
}