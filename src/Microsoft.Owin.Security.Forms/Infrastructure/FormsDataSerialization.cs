using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNet.Security.Services;
using Newtonsoft.Json;

namespace Microsoft.Owin.Security.Forms.Infrastructure
{
    public static class FormsDataSerialization
    {
        public static byte[] Serialize(FormsModel model)
        {
            var memory = new MemoryStream();
            using (var writer = new JsonTextWriter(new StreamWriter(memory)))
            {
                WriteData(writer, model);
            }
            return memory.ToArray();
        }

        public static FormsModel Deserialize(byte[] data)
        {
            try
            {
                using (var reader = new JsonTextReader(new StreamReader(new MemoryStream(data))))
                {
                    return reader.Read() ? ReadData(reader) : null;
                }
            }
            catch
            {
                return null;
            }
        }

        private static void WriteData(JsonTextWriter writer, FormsModel model)
        {
            writer
                .StartObject()
                .Property("p", model.IsPersistent, true)
                .Property("e", model.ExpireUtc, DateTimeOffset.MinValue)
                .StartArray("i");

            foreach (var identity in model.Principal.Identities)
            {
                writer
                    .StartObject()
                    .Property("at", identity.AuthenticationType, null)
                    .Property("nct", identity.NameClaimType, ClaimsIdentity.DefaultNameClaimType)
                    .Property("rct", identity.RoleClaimType, ClaimsIdentity.DefaultRoleClaimType)
                    .StartArray("c");

                foreach (var claim in identity.Claims)
                {
                    writer
                        .StartObject()
                        .Property("t", claim.Type, identity.NameClaimType)
                        .Property("v", claim.Value, null)
                        .EndObject();
                }

                writer
                    .EndArray()
                    .EndObject();
            }
        }

        private static FormsModel ReadData(JsonReader reader)
        {
            bool isPersistent;
            DateTimeOffset expireUtc;
            var principal = new ClaimsPrincipal();

            reader
                .StartObject()
                .Property("p", out isPersistent, true)
                .Property("e", out expireUtc, DateTimeOffset.MinValue)
                .StartArray("i");

            while (reader.TokenType != JsonToken.EndArray)
            {
                string authenticationType;
                string nameClaimType;
                string roleClaimType;
                var claims = new List<Claim>();

                reader
                    .StartObject()
                    .Property("at", out authenticationType, null)
                    .Property("nct", out nameClaimType, ClaimsIdentity.DefaultNameClaimType)
                    .Property("rct", out roleClaimType, ClaimsIdentity.DefaultRoleClaimType)
                    .StartArray("c");

                while (reader.TokenType != JsonToken.EndArray)
                {
                    string claimType;
                    string claimValue;
                    reader
                        .StartObject()
                        .Property("t", out claimType, nameClaimType)
                        .Property("v", out claimValue, null)
                        .EndObject();
                    claims.Add(new Claim(claimType, claimValue));
                }

                reader
                    .EndArray()
                    .EndObject();

                principal.AddIdentity(new ClaimsIdentity(
                    claims,
                    authenticationType,
                    nameClaimType,
                    roleClaimType));
            }

            reader
                .EndArray()
                .EndObject(final: true);

            return new FormsModel
            {
                IsPersistent = isPersistent,
                ExpireUtc = expireUtc,
                Principal = principal
            };
        }
    }
}