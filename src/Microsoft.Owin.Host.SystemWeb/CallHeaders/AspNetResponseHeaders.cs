// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
             // Content-Type is special, use response instead of headers
            if (Constants.ContentType.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                string contentType = _response.ContentType;
                if (string.IsNullOrEmpty(contentType))
                {
                    return null;
                }
                return new[] { contentType };
            }

            return _headers.GetValues(key);
        }

        private void Set(string key, string[] values)
        {
            // _headers.Set(key, values[0]); does not work with content-type or cache-control
            // if Write and Flush are called immediately after.
            if (Constants.ContentType.Equals(key, StringComparison.OrdinalIgnoreCase)
                || Constants.CacheControl.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                _headers.Remove(key);
                if (values != null)
                {
                    Append(key, values, 0);
                }
            }
            else
            {
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
            for (int i = 0; i < _headers.Count; i++)
            {
                string key = _headers.Keys[i];

                if (Constants.ContentType.Equals(key, StringComparison.OrdinalIgnoreCase))
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
