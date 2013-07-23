// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.IO;
using System.IO.Compression;
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
                using (var compression = new GZipStream(memory, CompressionLevel.Optimal))
                {
                    using (var writer = new BinaryWriter(compression))
                    {
                        Write(writer, model);
                    }
                }
                return memory.ToArray();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Dispose is idempotent")]
        public virtual AuthenticationTicket Deserialize(byte[] data)
        {
            using (var memory = new MemoryStream(data))
            {
                using (var compression = new GZipStream(memory, CompressionMode.Decompress))
                {
                    using (var reader = new BinaryReader(compression))
                    {
                        return Read(reader);
                    }
                }
            }
        }

        public static void Write(BinaryWriter writer, AuthenticationTicket model)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

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
            PropertiesSerializer.Write(writer, model.Properties);
        }

        public static AuthenticationTicket Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

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
            AuthenticationProperties properties = PropertiesSerializer.Read(reader);
            return new AuthenticationTicket(identity, properties);
        }
    }
}
