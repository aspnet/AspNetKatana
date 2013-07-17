// <copyright file="AuthenticationProperties.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthenticationProperties
    {
        internal const string IssuedUtcKey = ".issued";
        internal const string ExpiresUtcKey = ".expires";
        internal const string IsPersistentKey = ".persistent";
        internal const string RedirectUrlKey = ".redirect";
        internal const string UtcDateTimeFormat = "r";

        private readonly IDictionary<string, string> _dictionary;

        /// <summary>
        /// 
        /// </summary>
        public AuthenticationProperties() : this(null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dictionary"></param>
        public AuthenticationProperties(IDictionary<string, string> dictionary)
        {
            _dictionary = dictionary ?? new Dictionary<string, string>(StringComparer.Ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, string> Dictionary
        {
            get { return _dictionary; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsPersistent
        {
            get { return _dictionary.ContainsKey(IsPersistentKey); }
            set
            {
                if (_dictionary.ContainsKey(IsPersistentKey))
                {
                    if (!value)
                    {
                        _dictionary.Remove(IsPersistentKey);
                    }
                }
                else
                {
                    if (value)
                    {
                        _dictionary.Add(IsPersistentKey, string.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "By design")]
        public string RedirectUrl
        {
            get
            {
                string value;
                return _dictionary.TryGetValue(RedirectUrlKey, out value) ? value : null;
            }
            set
            {
                if (value != null)
                {
                    _dictionary[RedirectUrlKey] = value;
                }
                else
                {
                    if (_dictionary.ContainsKey(RedirectUrlKey))
                    {
                        _dictionary.Remove(RedirectUrlKey);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTimeOffset? IssuedUtc
        {
            get
            {
                string value;
                if (_dictionary.TryGetValue(IssuedUtcKey, out value))
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
                    _dictionary[IssuedUtcKey] = value.Value.ToString(UtcDateTimeFormat, CultureInfo.InvariantCulture);
                }
                else
                {
                    if (_dictionary.ContainsKey(IssuedUtcKey))
                    {
                        _dictionary.Remove(IssuedUtcKey);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTimeOffset? ExpiresUtc
        {
            get
            {
                string value;
                if (_dictionary.TryGetValue(ExpiresUtcKey, out value))
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
                    _dictionary[ExpiresUtcKey] = value.Value.ToString(UtcDateTimeFormat, CultureInfo.InvariantCulture);
                }
                else
                {
                    if (_dictionary.ContainsKey(ExpiresUtcKey))
                    {
                        _dictionary.Remove(ExpiresUtcKey);
                    }
                }
            }
        }
    }
}

#endif
