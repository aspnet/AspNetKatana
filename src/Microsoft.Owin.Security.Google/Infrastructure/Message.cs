// <copyright file="Message.cs" company="Microsoft Open Technologies, Inc.">
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

        public Dictionary<string, Property> Namespaces { get; private set; }
        public Dictionary<string, Property> Properties { get; private set; }

        private void Add(IEnumerable<KeyValuePair<string, string[]>> query)
        {
            Property[] data = query.Select(kv => new Property
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
}
