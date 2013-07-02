// <copyright file="AspNetResponseHeaders.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Microsoft.Owin.Host.SystemWeb.CallHeaders
{
    // PERF: This class is an IDictionary facade to enable direct enumeration from original NameValueCollection headers.
    internal class AspNetResponseHeaders : IDictionary<string, string[]>
    {
        private readonly HttpResponseBase _response;
        private readonly NameValueCollection _headers;
        
        internal AspNetResponseHeaders(HttpResponseBase response)
        {
            _response = response;
            _headers = response.Headers;
        }

        public ICollection<string> Keys
        {
            get { return _headers.AllKeys; }
        }

        public ICollection<string[]> Values
        {
            get { return _headers.AllKeys.Select(key => _headers.GetValues(key)).ToList(); }
        }

        public int Count
        {
            get { return _headers.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public string[] this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                string[] values = Get(key);
                if (values == null)
                {
                    throw new KeyNotFoundException(key);
                }
                return values;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                Set(key, value);
            }
        }

        private string[] Get(string key)
        {
            // Content-Type and Cache-Control are special, use response instead of headers
            if (Constants.ContentType.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                string contentType = _response.ContentType;
                if (string.IsNullOrEmpty(contentType))
                {
                    return null;
                }
                return new[] { contentType };
            }

            if (Constants.CacheControl.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                string cacheControl = _response.CacheControl;
                if (string.IsNullOrEmpty(cacheControl))
                {
                    return null;
                }
                return new[] { cacheControl };
            }

            return _headers.GetValues(key);
        }

        private void Set(string key, string[] values)
        {
            if (Constants.ContentType.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                _headers.Remove(key);
                if (values == null || values.Length == 0)
                {
                    _response.ContentType = null;
                }
                else
                {
                    _response.ContentType = values[0];
                    Append(key, values, offset: 1);
                }
                return;
            }

            if (Constants.CacheControl.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                _headers.Remove(key);
                if (values == null || values.Length == 0)
                {
                    _response.CacheControl = null;
                }
                else
                {
                    _response.CacheControl = values[0];
                    Append(key, values, offset: 1);
                }
                return;
            }

            if (values == null || values.Length == 0)
            {
                _headers.Remove(key);
            }
            else 
            {
                _headers.Set(key, values[0]);
                Append(key, values, 1);
            }
        }

        public void Add(KeyValuePair<string, string[]> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(string key, string[] value)
        {
            if (ContainsKey(key))
            {
                // IDictionary contract
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_DuplicateKey, key));
            }
            Append(key, value, 0);
        }

        private void Append(string key, string[] value, int offset)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            for (; offset < value.Length; offset++)
            {
                _response.AppendHeader(key, value[offset]);
            }
        }

        public bool ContainsKey(string key)
        {
            if (Constants.ContentType.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return !string.IsNullOrEmpty(_response.ContentType);
            }
            if (Constants.CacheControl.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return !string.IsNullOrEmpty(_response.CacheControl);
            }

            return Keys.Contains(key, StringComparer.OrdinalIgnoreCase);
        }

        public bool Remove(string key)
        {
            if (ContainsKey(key))
            {
                Set(key, null);
                return true;
            }
            return false;
        }

        public bool TryGetValue(string key, out string[] value)
        {
            value = Get(key);
            return value != null;
        }

        public void Clear()
        {
            _response.ClearHeaders();
            _headers.Clear();
        }

        public bool Contains(KeyValuePair<string, string[]> item)
        {
            string[] value;
            return TryGetValue(item.Key, out value) && ReferenceEquals(item.Value, value);
        }

        public void CopyTo(KeyValuePair<string, string[]>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (arrayIndex > Count - array.Length)
            {
                throw new ArgumentOutOfRangeException("arrayIndex", arrayIndex, string.Empty);
            }

            foreach (var item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        public bool Remove(KeyValuePair<string, string[]> item)
        {
            return Contains(item) && Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            if (!string.IsNullOrEmpty(_response.ContentType))
            {
                yield return new KeyValuePair<string, string[]>(Constants.ContentType, new[] { _response.ContentType });
            }
            if (!string.IsNullOrEmpty(_response.CacheControl))
            {
                yield return new KeyValuePair<string, string[]>(Constants.CacheControl, new[] { _response.CacheControl });
            }

            for (int i = 0; i < _headers.Count; i++)
            {
                string key = _headers.Keys[i];
                if (Constants.ContentType.Equals(key, StringComparison.OrdinalIgnoreCase)
                    || Constants.CacheControl.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    // May be duplicated in the properties and collection.
                    continue;
                }

                yield return new KeyValuePair<string, string[]>(key, _headers.GetValues(i));
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, string[]>>)this).GetEnumerator();
        }
    }
}
