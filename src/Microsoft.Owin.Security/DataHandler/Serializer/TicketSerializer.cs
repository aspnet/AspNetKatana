// <copyright file="TicketSerializer.cs" company="Microsoft Open Technologies, Inc.">
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

#if NET45

using System.IO;
using System.Linq;
using System.Security.Claims;

namespace Microsoft.Owin.Security.DataHandler.Serializer
{
    public class TicketSerializer : IDataSerializer<AuthenticationTicket>
    {
        private const int FormatVersion = 1;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Dispose is idempotent")]
        public virtual byte[] Serialize(AuthenticationTicket model)
        {
            using (var memory = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memory))
                {
                    Write(writer, model);
                    writer.Flush();
                    return memory.ToArray();
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Dispose is idempotent")]
        public virtual AuthenticationTicket Deserialize(byte[] data)
        {
            using (var memory = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(memory))
                {
                    return Read(reader);
                }
            }
        }

        public static void Write(BinaryWriter writer, AuthenticationTicket model)
        {
            writer.Write(FormatVersion);
            ClaimsIdentity identity = model.Identity;
            writer.Write(identity.AuthenticationType);
            writer.Write(identity.NameClaimType);
            writer.Write(identity.RoleClaimType);
            writer.Write(identity.Claims.Count());
            foreach (var claim in identity.Claims)
            {
                writer.Write(claim.Type);
                writer.Write(claim.Value);
                writer.Write(claim.ValueType);
                writer.Write(claim.Issuer);
            }
            ExtraSerializer.Write(writer, model.Extra);
        }

        public static AuthenticationTicket Read(BinaryReader reader)
        {
            if (reader.ReadInt32() != FormatVersion)
            {
                return null;
            }

            string authenticationType = reader.ReadString();
            string nameClaimType = reader.ReadString();
            string roleClaimType = reader.ReadString();
            int count = reader.ReadInt32();
            var claims = new Claim[count];
            for (int index = 0; index != count; ++index)
            {
                string type = reader.ReadString();
                string value = reader.ReadString();
                string valueType = reader.ReadString();
                string issuer = reader.ReadString();
                claims[index] = new Claim(type, value, valueType, issuer);
            }
            var identity = new ClaimsIdentity(claims, authenticationType, nameClaimType, roleClaimType);
            AuthenticationExtra extra = ExtraSerializer.Read(reader);
            return new AuthenticationTicket(identity, extra);
        }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
