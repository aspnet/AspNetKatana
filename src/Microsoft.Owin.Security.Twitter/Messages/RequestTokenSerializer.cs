// <copyright file="RequestTokenSerializer.cs" company="Microsoft Open Technologies, Inc.">
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

using System.IO;
using Microsoft.Owin.Security.DataHandler.Serializer;

namespace Microsoft.Owin.Security.Twitter.Messages
{
    public class RequestTokenSerializer : IDataSerializer<RequestToken>
    {
        private const int FormatVersion = 1;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Dispose is idempotent")]
        public virtual byte[] Serialize(RequestToken token)
        {
            using (var memory = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memory))
                {
                    Write(writer, token);
                    writer.Flush();
                    return memory.ToArray();
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Dispose is idempotent")]
        public virtual RequestToken Deserialize(byte[] data)
        {
            using (var memory = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(memory))
                {
                    return Read(reader);
                }
            }
        }

        public static void Write(BinaryWriter writer, RequestToken token)
        {
            writer.Write(FormatVersion);
            writer.Write(token.Token);
            writer.Write(token.TokenSecret);
            writer.Write(token.CallbackConfirmed);
        }

        public static RequestToken Read(BinaryReader reader)
        {
            if (reader.ReadInt32() != FormatVersion)
            {
                return null;
            }

            var token = reader.ReadString();
            var tokenSecret = reader.ReadString();
            var callbackConfirmed = reader.ReadBoolean();

            return new RequestToken { Token = token, TokenSecret = tokenSecret, CallbackConfirmed = callbackConfirmed };
        }
    }
}
