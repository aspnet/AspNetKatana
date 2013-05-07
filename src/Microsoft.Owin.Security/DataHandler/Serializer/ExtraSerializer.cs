// <copyright file="ExtraSerializer.cs" company="Microsoft Open Technologies, Inc.">
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

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Owin.Security.DataHandler.Serializer
{
    public class ExtraSerializer : IDataSerializer<AuthenticationExtra>
    {
        private const int FormatVersion = 1;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Dispose is idempotent")]
        public byte[] Serialize(AuthenticationExtra extra)
        {
            using (var memory = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memory))
                {
                    Write(writer, extra);
                    writer.Flush();
                    return memory.ToArray();
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Dispose is idempotent")]
        public AuthenticationExtra Deserialize(byte[] data)
        {
            using (var memory = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(memory))
                {
                    return Read(reader);
                }
            }
        }

        public static void Write(BinaryWriter writer, AuthenticationExtra extra)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (extra == null)
            {
                throw new ArgumentNullException("extra");
            }

            writer.Write(FormatVersion);
            writer.Write(extra.Properties.Count);
            foreach (var kv in extra.Properties)
            {
                writer.Write(kv.Key);
                writer.Write(kv.Value);
            }
        }

        public static AuthenticationExtra Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            if (reader.ReadInt32() != FormatVersion)
            {
                return null;
            }
            int count = reader.ReadInt32();
            var extra = new Dictionary<string, string>(count);
            for (int index = 0; index != count; ++index)
            {
                string key = reader.ReadString();
                string value = reader.ReadString();
                extra.Add(key, value);
            }
            return new AuthenticationExtra(extra);
        }
    }
}
