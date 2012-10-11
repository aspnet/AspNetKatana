// <copyright file="DictionaryExtensions.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
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

using System.Linq;

namespace System.Collections.Generic
{
    internal static class DictionaryExtensions
    {
        internal static T Get<T>(this IDictionary<string, object> dictionary, string key)
        {
            object value;
            return dictionary.TryGetValue(key, out value) ? (T)value : default(T);
        }

        internal static string[] GetHeaders(this IDictionary<string, string[]> headers, string name)
        {
            string[] value;
            return headers != null && headers.TryGetValue(name, out value) ? value : null;
        }

        internal static string GetHeader(this IDictionary<string, string[]> headers, string name)
        {
            var values = GetHeaders(headers, name);
            if (values == null)
            {
                return null;
            }

            switch (values.Length)
            {
                case 0:
                    return string.Empty;
                case 1:
                    return values[0];
                default:
                    return string.Join(",", values);
            }
        }

        internal static IDictionary<string, string[]> AppendHeader(this IDictionary<string, string[]> headers,
            string name, string value)
        {
            return AppendHeader(headers, name, new[] { value });
        }

        internal static IDictionary<string, string[]> AppendHeader(this IDictionary<string, string[]> headers,
            string name, string[] value)
        {
            string[] values;
            if (headers.TryGetValue(name, out values))
            {
                headers[name] = values.Concat(value).ToArray();
            }
            else
            {
                headers[name] = value;
            }
            return headers;
        }
    }
}
