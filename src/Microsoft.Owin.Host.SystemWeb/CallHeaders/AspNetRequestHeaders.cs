// <copyright file="AspNetRequestHeaders.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Microsoft.Owin.Host.SystemWeb.CallHeaders
{
    // PERF: This class is an IDictionary facade to enable direct enumeration from original NameValueCollection headers.
    internal class AspNetRequestHeaders : IDictionary<string, string[]>
    {
        private readonly NameValueCollection _headers;

        internal AspNetRequestHeaders(NameValueCollection headers)
        {
            _headers = headers;
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
                if (!ContainsKey(key))
                {
                    throw new KeyNotFoundException();
                }
                return _headers.GetValues(key);
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                _headers.Remove(key);
                Add(key, value);
            }
        }

        public void Add(string key, string[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            foreach (string v in value)
            {
                _headers.Add(key, v);
            }
        }

        public bool ContainsKey(string key)
        {
            return _headers.AllKeys.Contains(key, StringComparer.OrdinalIgnoreCase);
        }

        public void Add(KeyValuePair<string, string[]> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Remove(string key)
        {
            if (ContainsKey(key))
            {
                _headers.Remove(key);
                return true;
            }
            return false;
        }

        public bool TryGetValue(string key, out string[] value)
        {
            if (ContainsKey(key))
            {
                value = _headers.GetValues(key);
                return true;
            }
            value = null;
            return false;
        }

        public void Clear()
        {
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
            for (int i = 0; i < _headers.Count; i++)
            {
                yield return new KeyValuePair<string, string[]>(_headers.Keys[i], _headers.GetValues(i));
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, string[]>>)this).GetEnumerator();
        }
    }
}
