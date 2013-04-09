using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Microsoft.AspNet.Security.Services
{
    internal static class JsonWriterExtensions
    {
        public static JsonWriter StartObject(this JsonWriter writer)
        {
            writer.WriteStartObject();
            return writer;
        }

        public static JsonWriter EndObject(this JsonWriter writer)
        {
            writer.WriteEndObject();
            return writer;
        }

        public static JsonWriter Property(this JsonWriter writer, string name, string value, string defaultValue)
        {
            if (!String.Equals(value, defaultValue, StringComparison.Ordinal))
            {
                writer.WritePropertyName(name);
                writer.WriteValue(value);
            }
            return writer;
        }

        public static JsonWriter Property(this JsonWriter writer, string name, bool value, bool defaultValue)
        {
            if (value != defaultValue)
            {
                writer.WritePropertyName(name);
                writer.WriteValue(value ? 1 : 0);
            }
            return writer;
        }

        public static JsonWriter Property(this JsonWriter writer, string name, DateTimeOffset value, DateTimeOffset defaultValue)
        {
            if (value != defaultValue)
            {
                writer.WritePropertyName(name);
                writer.WriteValue(value);
            }
            return writer;
        }

        public static JsonWriter StartArray(this JsonWriter writer, string name)
        {
            writer.WritePropertyName(name);
            writer.WriteStartArray();
            return writer;
        }

        public static JsonWriter EndArray(this JsonWriter writer)
        {
            writer.WriteEndArray();
            return writer;
        }
    }
}