// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

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
        internal const string RedirectUriKey = ".redirect";
        internal const string UtcDateTimeFormat = "r";

        private readonly IDictionary<string, string> _dictionary;

        /// <summary>
        /// 
        /// </summary>
        public AuthenticationProperties()
        {
            _dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
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
        public string RedirectUri
        {
            get
            {
                string value;
                return _dictionary.TryGetValue(RedirectUriKey, out value) ? value : null;
            }
            set
            {
                if (value != null)
                {
                    _dictionary[RedirectUriKey] = value;
                }
                else
                {
                    if (_dictionary.ContainsKey(RedirectUriKey))
                    {
                        _dictionary.Remove(RedirectUriKey);
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

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
