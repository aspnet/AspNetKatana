// <copyright file="AuthenticationExtra.cs" company="Microsoft Open Technologies, Inc.">
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

#if NET45

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Owin.Security
{
    public class AuthenticationExtra
    {
        internal const string IssuedUtcKey = ".issued";
        internal const string ExpiresUtcKey = ".expires";
        internal const string IsPersistentKey = ".persistent";
        internal const string RedirectUrlKey = ".redirect";
        internal const string UtcDateTimeFormat = "r";

        private readonly IDictionary<string, string> _properties;

        public AuthenticationExtra()
        {
            _properties = new Dictionary<string, string>(StringComparer.Ordinal);
        }

        public AuthenticationExtra(IDictionary<string, string> properties)
        {
            _properties = properties ?? new Dictionary<string, string>(StringComparer.Ordinal);
        }

        public IDictionary<string, string> Properties
        {
            get { return _properties; }
        }

        public bool IsPersistent
        {
            get { return _properties.ContainsKey(IsPersistentKey); }
            set
            {
                if (_properties.ContainsKey(IsPersistentKey))
                {
                    if (!value)
                    {
                        _properties.Remove(IsPersistentKey);
                    }
                }
                else
                {
                    if (value)
                    {
                        _properties.Add(IsPersistentKey, string.Empty);
                    }
                }
            }
        }

        public string RedirectUrl
        {
            get
            {
                string value;
                return _properties.TryGetValue(RedirectUrlKey, out value) ? value : null;
            }
            set
            {
                if (value != null)
                {
                    _properties[RedirectUrlKey] = value;
                }
                else
                {
                    if (_properties.ContainsKey(RedirectUrlKey))
                    {
                        _properties.Remove(RedirectUrlKey);
                    }
                }
            }
        }

        public DateTimeOffset? IssuedUtc
        {
            get
            {
                string value;
                if (_properties.TryGetValue(IssuedUtcKey, out value))
                {
                    DateTimeOffset dateTimeOffset;
                    if (DateTimeOffset.TryParseExact(value, UtcDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTimeOffset))
                    {
                        return dateTimeOffset;
                    }
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    _properties[IssuedUtcKey] = value.Value.ToString(UtcDateTimeFormat, CultureInfo.InvariantCulture);
                }
                else
                {
                    if (_properties.ContainsKey(IssuedUtcKey))
                    {
                        _properties.Remove(IssuedUtcKey);
                    }
                }
            }
        }

        public DateTimeOffset? ExpiresUtc
        {
            get
            {
                string value;
                if (_properties.TryGetValue(ExpiresUtcKey, out value))
                {
                    DateTimeOffset dateTimeOffset;
                    if (DateTimeOffset.TryParseExact(value, UtcDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTimeOffset))
                    {
                        return dateTimeOffset;
                    }
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    _properties[ExpiresUtcKey] = value.Value.ToString(UtcDateTimeFormat, CultureInfo.InvariantCulture);
                }
                else
                {
                    if (_properties.ContainsKey(ExpiresUtcKey))
                    {
                        _properties.Remove(ExpiresUtcKey);
                    }
                }
            }
        }
    }
}

#endif
