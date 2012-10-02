using System.Linq;
using System.Collections.Generic;

namespace System.Collections.Generic
{
    static class DictionaryExtensions
    {
        public static T Get<T>(this IDictionary<string, object> dictionary, string key)
        {
            object value;
            return dictionary.TryGetValue(key, out value) ? (T)value : default(T);
        }

        public static string[] GetHeaders(this IDictionary<string, string[]> headers, string name)
        {
            string[] value;
            return headers != null && headers.TryGetValue(name, out value) ? value : null;
        }

        public static string GetHeader(this IDictionary<string, string[]> headers, string name)
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

        public static IDictionary<string, string[]> AppendHeader(this IDictionary<string, string[]> headers,
            string name, string value)
        {
            return AppendHeader(headers, name, new[] { value });
        }

        public static IDictionary<string, string[]> AppendHeader(this IDictionary<string, string[]> headers,
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