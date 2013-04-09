using System;
using Newtonsoft.Json;

namespace Microsoft.AspNet.Security.Services
{
    internal static class JsonReaderExtensions
    {
        public static JsonReader StartObject(this JsonReader reader)
        {
            if (reader.TokenType == JsonToken.StartObject && reader.Read())
            {
                return reader;
            }
            throw new JsonSerializationException();
        }

        public static JsonReader EndObject(this JsonReader reader, bool final = false)
        {
            if (reader.TokenType == JsonToken.EndObject && (reader.Read() || final))
            {
                return reader;
            }
            throw new JsonSerializationException();
        }

        public static JsonReader Property(this JsonReader reader, string name, out string value, string defaultValue)
        {
            if (reader.TokenType == JsonToken.PropertyName &&
                string.Equals((string)reader.Value, name, StringComparison.Ordinal))
            {
                value = reader.ReadAsString();
                reader.Read();
            }
            else
            {
                value = defaultValue;
            }
            return reader;
        }

        public static JsonReader Property(this JsonReader reader, string name, out bool value, bool defaultValue)
        {
            if (reader.TokenType == JsonToken.PropertyName &&
                string.Equals((string)reader.Value, name, StringComparison.Ordinal))
            {
                value = reader.ReadAsInt32() == 1;
                reader.Read();
            }
            else
            {
                value = defaultValue;
            }
            return reader;
        }

        public static JsonReader Property(this JsonReader reader, string name, out DateTimeOffset value, DateTimeOffset defaultValue)
        {
            if (reader.TokenType == JsonToken.PropertyName &&
                string.Equals((string)reader.Value, name, StringComparison.Ordinal))
            {
                var readValue = reader.ReadAsDateTimeOffset();
                value = readValue.HasValue ? readValue.Value : defaultValue;
                reader.Read();
            }
            else
            {
                value = defaultValue;
            }
            return reader;
        }

        public static JsonReader StartArray(this JsonReader reader, string name)
        {
            if (reader.TokenType == JsonToken.PropertyName &&
                string.Equals((string)reader.Value, name, StringComparison.Ordinal) &&
                reader.Read() &&
                reader.TokenType == JsonToken.StartArray &&
                reader.Read())
            {
                return reader;
            }
            throw new JsonSerializationException();
        }

        public static JsonReader EndArray(this JsonReader reader)
        {
            if (reader.TokenType == JsonToken.EndArray && reader.Read())
            {
                return reader;
            }
            throw new JsonSerializationException();
        }
    }
}