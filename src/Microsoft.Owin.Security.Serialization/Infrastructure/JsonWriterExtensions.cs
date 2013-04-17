// <copyright file="JsonWriterExtensions.cs" company="Microsoft Open Technologies, Inc.">
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
    internal static class JsonWriterExtensions
    {
        public static JsonWriter StartObject(this JsonWriter writer)
        {
            writer.WriteStartObject();
            return writer;
        }

        public static JsonWriter StartObject(this JsonWriter writer, string name)
        {
            writer.WritePropertyName(name);
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
