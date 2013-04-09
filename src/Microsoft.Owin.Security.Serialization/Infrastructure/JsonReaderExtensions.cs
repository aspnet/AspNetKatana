// <copyright file="JsonReaderExtensions.cs" company="Microsoft Open Technologies, Inc.">
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
using Newtonsoft.Json;

namespace Microsoft.Owin.Security.Serialization.Infrastructure
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

        public static JsonReader StartObject(this JsonReader reader, string name)
        {
            if (reader.TokenType == JsonToken.PropertyName &&
                string.Equals((string)reader.Value, name, StringComparison.Ordinal) &&
                reader.Read() &&
                reader.TokenType == JsonToken.StartObject &&
                reader.Read())
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

        public static JsonReader Property(this JsonReader reader, out string name, out string value)
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                name = (string)reader.Value;
                value = reader.ReadAsString();
                reader.Read();
                return reader;
            }
            throw new JsonSerializationException();
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
                DateTimeOffset? readValue = reader.ReadAsDateTimeOffset();
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
