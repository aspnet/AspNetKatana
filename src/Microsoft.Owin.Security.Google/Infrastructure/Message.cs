using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Owin.Security.Google.Infrastructure
{
    internal class Message
    {
        public Message(IDictionary<string, string[]> query)
        {
            Namespaces = new Dictionary<string, Property>(StringComparer.Ordinal);
            Properties = new Dictionary<string, Property>(query.Count, StringComparer.Ordinal);
            Add(query);
        }

        private void Add(IEnumerable<KeyValuePair<string, string[]>> query)
        {
            var data = query.Select(kv => new Property
            {
                Key = kv.Key,
                Name = kv.Key + ".",
                Value = string.Join(",", kv.Value)
            }).ToArray();

            foreach (var item in data)
            {
                if (item.Name.StartsWith("openid.ns.", StringComparison.Ordinal))
                {
                    item.Namespace = item.Value;
                    item.Name = "openid." + item.Name.Substring("openid.ns.".Length);
                    Namespaces.Add(item.Namespace, item);
                }
            }
            foreach (var item in data)
            {
                if (item.Namespace == null)
                {
                    Property match = null;
                    foreach (var ns in Namespaces.Values)
                    {
                        if ((match == null || match.Name.Length < ns.Name.Length) &&
                            item.Name.StartsWith(ns.Name, StringComparison.Ordinal))
                        {
                            match = ns;
                        }
                    }
                    if (match != null)
                    {
                        item.Namespace = match.Namespace;
                        item.Name = item.Name.Substring(match.Name.Length);
                    }
                    Properties.Add(item.Name + item.Namespace, item);
                }
            }
        }

        public Dictionary<string, Property> Namespaces { get; private set; }
        public Dictionary<string, Property> Properties { get; private set; }

        public bool TryGetValue(string key, out string mode)
        {
            Property property;
            if (Properties.TryGetValue(key, out property))
            {
                mode = property.Value;
                return true;
            }
            mode = null;
            return false;
        }

        public string ToFormUrlEncoded()
        {
            bool first = true;
            var sb = new StringBuilder();
            foreach (var kv in Namespaces.Concat(Properties))
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append('&');
                }
                sb.Append(Uri.EscapeDataString(kv.Value.Key));
                sb.Append('=');
                sb.Append(Uri.EscapeDataString(kv.Value.Value));
            }
            return sb.ToString();
        }
    }

    internal class Property
    {
        public string Key { get; set; }
        public string Namespace { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
