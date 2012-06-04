using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gate
{
    internal static class Headers
    {
        public static IDictionary<string, IEnumerable<string>> New()
        {
            return new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);
        }

        public static IDictionary<string, IEnumerable<string>> New(IDictionary<string, IEnumerable<string>> headers)
        {
            if (headers == null)
                return New();

            return new Dictionary<string, IEnumerable<string>>(headers, StringComparer.OrdinalIgnoreCase);
        }

        public static bool HasHeader(this IDictionary<string, IEnumerable<string>> headers,
            string name)
        {
            IEnumerable<string> values;
            if (!headers.TryGetValue(name, out values) || values == null)
                return false;
            return values.Any(value => !string.IsNullOrWhiteSpace(value));
        }

        public static IDictionary<string, IEnumerable<string>> SetHeader(this IDictionary<string, IEnumerable<string>> headers,
            string name, string value)
        {
            headers[name] = new[] { value };
            return headers;
        }

        public static IDictionary<string, IEnumerable<string>> SetHeader(this IDictionary<string, IEnumerable<string>> headers,
            string name, IEnumerable<string> values)
        {
            headers[name] = values;
            return headers;
        }

        public static IDictionary<string, IEnumerable<string>> AddHeader(this IDictionary<string, IEnumerable<string>> headers,
            string name, string value)
        {
            return AddHeader(headers, name, new[] {value});
        }

        public static IDictionary<string, IEnumerable<string>> AddHeader(this IDictionary<string, IEnumerable<string>> headers,
            string name, IEnumerable<string> value)
        {
            IEnumerable<string> values;
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

        public static IEnumerable<string> GetHeaders(this IDictionary<string, IEnumerable<string>> headers,
            string name)
        {
            IEnumerable<string> value;
            return headers != null && headers.TryGetValue(name, out value) ? value : null;
        }

        public static string GetHeader(this IDictionary<string, IEnumerable<string>> headers,
            string name)
        {
            var values = GetHeaders(headers, name);
            if (values == null)
            {
                return null;
            }

            if (values is string[])
            {
                var valueArray = (string[])values;
                switch (valueArray.Length)
                {
                    case 0:
                        return string.Empty;
                    case 1:
                        return valueArray[0];
                    default:
                        return string.Join(",", valueArray);
                }
            }

            var enumerator = values.GetEnumerator();
            if (!enumerator.MoveNext())
                return string.Empty;

            var string1 = enumerator.Current;
            if (!enumerator.MoveNext())
                return string1;

            var string2 = enumerator.Current;
            if (!enumerator.MoveNext())
                return string1 + "," + string2;

            var sb = new StringBuilder(string1 + "," + string2 + "," + enumerator.Current);
            while (enumerator.MoveNext())
            {
                sb.Append(',');
                sb.Append(enumerator.Current);
            }
            return sb.ToString();
        }
    }
}
