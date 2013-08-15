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
    internal class AspNetRequestHeaders : IDictionary<string, string[]>
    {
        private readonly HttpRequestBase _httpRequest;

        internal AspNetRequestHeaders(HttpRequestBase httpRequest)
        {
            _httpRequest = httpRequest;
        }

        public ICollection<string> Keys
        {
            get { return Headers.AllKeys; }
        }

        public ICollection<string[]> Values
        {
            get { return Headers.AllKeys.Select(key => Headers.GetValues(key)).ToList(); }
        }

        public int Count
        {
            get { return Headers.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        private NameValueCollection Headers
        {
            get { return _httpRequest.Headers; }
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
            return Headers.GetValues(key);
        }

        private void Set(string key, string[] values)
        {
            if (values == null || values.Length == 0)
            {
                Headers.Remove(key);
            }
            else
            {
                Headers.Set(key, values[0]);
                Add(key, values, 1);
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
            Add(key, value, 0);
        }

        private void Add(string key, string[] value, int offset)
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
                Headers.Add(key, value[offset]);
            }
        }

        public bool ContainsKey(string key)
        {
            return Keys.Contains(key, StringComparer.OrdinalIgnoreCase);
        }

        public bool Remove(string key)
        {
            if (ContainsKey(key))
            {
                Headers.Remove(key);
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
            Headers.Clear();
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
            for (int i = 0; i < Headers.Count; i++)
            {
                yield return new KeyValuePair<string, string[]>(Headers.Keys[i], Headers.GetValues(i));
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, string[]>>)this).GetEnumerator();
        }
    }
}
