// <copyright file="ExtraDictionarySerializer.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Microsoft.Owin.Security.ModelSerializer
{
    public class ExtraDictionarySerializer : IModelSerializer<IDictionary<string, string>>
    {
        public byte[] Serialize(IDictionary<string, string> extra)
        {
            using (var memory = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memory))
                {
                    writer.Write(1);
                    writer.Write(extra.Count);
                    foreach (var kv in extra)
                    {
                        writer.Write(kv.Key);
                        writer.Write(kv.Value);
                    }
                    writer.Flush();
                    return memory.ToArray();
                }
            }
        }

        public IDictionary<string, string> Deserialize(byte[] data)
        {
            using (var memory = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(memory))
                {
                    int version = reader.ReadInt32();
                    if (version != 1)
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
                    return extra;
                }
            }
        }
    }
}
