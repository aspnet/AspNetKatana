// <copyright file="DataModelSerialization.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using Microsoft.Owin.Security.Formatter.Infrastructure;
using Microsoft.Owin.Security.Serialization.Infrastructure;
using Newtonsoft.Json;

namespace Microsoft.Owin.Security.Serialization
{
    public static class DataModelSerialization
    {
        public static byte[] Serialize(DataModel model)
        {
            var memory = new MemoryStream();
            using (var writer = new JsonTextWriter(new StreamWriter(memory)))
            {
                WriteData(writer, model);
            }
            return memory.ToArray();
        }

        public static DataModel Deserialize(byte[] data)
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

        private static void WriteData(JsonTextWriter writer, DataModel model)
        {
            writer
                .StartObject()
                .StartObject("e");

            if (model.Extra != null)
            {
                foreach (var property in model.Extra)
                {
                    writer.Property(property.Key, property.Value, null);
                }
            }

            writer
                .EndObject()
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

            writer
                .EndArray()
                .EndObject();
        }

        private static DataModel ReadData(JsonReader reader)
        {
            var extra = new Dictionary<string, string>();
            var principal = new ClaimsPrincipal();

            reader
                .StartObject()
                .StartObject("e");

            while (reader.TokenType != JsonToken.EndObject)
            {
                string name;
                string value;
                reader.Property(out name, out value);
                extra.Add(name, value);
            }

            reader
                .EndObject()
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

            return new DataModel(principal, extra);
        }
    }
}
