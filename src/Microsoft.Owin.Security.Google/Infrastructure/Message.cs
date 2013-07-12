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
        public Message(IReadableStringCollection parameters, bool strict)
        {
            Namespaces = new Dictionary<string, Property>(StringComparer.Ordinal);
            Properties = new Dictionary<string, Property>(parameters.Count(), StringComparer.Ordinal);
            Add(parameters, strict);
        }

        public Dictionary<string, Property> Namespaces { get; private set; }
        public Dictionary<string, Property> Properties { get; private set; }

        /// <summary>
        /// Adds the openid parameters from querystring or form body into Namespaces and Properties collections.
        /// This normalizes the parameter name, by replacing the variable namespace alias with the 
        /// actual namespace in the collection's key, and will optionally skip any parameters that are
        /// not signed if the strict argument is true.
        /// </summary>
        /// <param name="parameters">The keys and values of the incoming querystring or form body</param>
        /// <param name="strict">True if keys that are not signed should be ignored</param>
        private void Add(IReadableStringCollection parameters, bool strict)
        {
            IEnumerable<KeyValuePair<string, string[]>> addingParameters;

            // strict is true if keys that are not signed should be strict
            if (strict)
            {
                IList<string> signed = parameters.GetValues("openid.signed");
                if (signed == null ||
                    signed.Count != 1)
                {
                    // nothing is added if the signed parameter is not present
                    return;
                }

                // determine the set of keys that are signed, or which may be used without
                // signing. ns, mode, signed, and sig each may be used without signing.
                var strictKeys = new HashSet<string>(signed[0]
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => "openid." + value)
                    .Concat(new[] { "openid.ns", "openid.mode", "openid.signed", "openid.sig" }));

                // the parameters to add are only the parameters what are in this set
                addingParameters = parameters.Where(kv => strictKeys.Contains(kv.Key));
            }
            else
            {
                // when strict is false all of the incoming parameters are to be added 
                addingParameters = parameters;
            }

            // convert the incoming parameter strings into Property objects. the
            // Key is the raw key name. The Name starts of being equal to Key with a 
            // trailing dot appended. The Value is the query or form value, with a comma delimiter
            // inserted between multiply occuring values.
            Property[] addingProperties = addingParameters.Select(kv => new Property
                {
                    Key = kv.Key,
                    Name = kv.Key + ".",
                    Value = string.Join(",", kv.Value)
                }).ToArray();

            // first, recognize which parameters are namespace declarations 

            var namespacePrefixes = new Dictionary<string, Property>(StringComparer.Ordinal);
            foreach (var item in addingProperties)
            {
                // namespaces appear as with "openid.ns" or "openid.ns.alias"
                if (item.Name.StartsWith("openid.ns.", StringComparison.Ordinal))
                {
                    // the value of the parameter is the uri of the namespace
                    item.Namespace = item.Value;
                    item.Name = "openid." + item.Name.Substring("openid.ns.".Length);

                    // the namespaces collection is keyed by the ns uri
                    Namespaces.Add(item.Namespace, item);

                    // and the prefixes collection is keyed by "openid.alias." 
                    namespacePrefixes.Add(item.Name, item);
                }
            }

            // second, recognize which parameters are property values 

            foreach (var item in addingProperties)
            {
                // anything with a namespace was already added to Namespaces
                if (item.Namespace == null)
                {
                    // look for the namespace match for this property. 
                    Property match = null;

                    // try finding where openid.alias.arg2 matches openid.ns.alies namespace
                    if (item.Name.StartsWith("openid.", StringComparison.Ordinal))
                    {
                        var dotIndex = item.Name.IndexOf('.', "openid.".Length);
                        if (dotIndex != -1)
                        {
                            var namespacePrefix = item.Name.Substring(0, dotIndex + 1);
                            namespacePrefixes.TryGetValue(namespacePrefix, out match);
                        }
                    }

                    // then try finding where openid.arg1 should match openid.ns namespace
                    if (match == null)
                    {
                        namespacePrefixes.TryGetValue("openid.", out match);
                    }

                    // when a namespace is found
                    if (match != null)
                    {
                        // the property's namespace is defined, and the namespace's prefix is removed
                        item.Namespace = match.Namespace;
                        item.Name = item.Name.Substring(match.Name.Length);
                    }

                    // the resulting property key is keyed by the local name and namespace
                    // so "openid.arg1" becomes "arg1.namespace-uri-of-openid"
                    // and "openid.alias.arg2" becomes "arg2.namespace-uri-of-alias"
                    Properties.Add(item.Name + item.Namespace, item);
                }
            }
        }

        public bool TryGetValue(string key, out string value)
        {
            Property property;
            if (Properties.TryGetValue(key, out property))
            {
                value = property.Value;
                return true;
            }
            value = null;
            return false;
        }

        public IEnumerable<KeyValuePair<string, string>> ToFormValues()
        {
            return Namespaces.Concat(Properties).Select(pair => new KeyValuePair<string, string>(pair.Value.Key, pair.Value.Value));
        }
    }
}
