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