// <copyright file="HeadersDictionaryBase.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Microsoft.Owin.Host.HttpListener.RequestProcessing
{
    internal abstract class HeadersDictionaryBase : IDictionary<string, string[]>
    {
        protected HeadersDictionaryBase()
        {
        }

        protected virtual WebHeaderCollection Headers { get; set; }

        public ICollection<string> Keys
        {
            get { return Headers.AllKeys; }
        }

        public ICollection<string[]> Values
        {
            get { return this.Select(pair => pair.Value).ToList(); }
        }

        public int Count
        {
            get { return Headers.AllKeys.Count(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public string[] this[string key]
        {
            get { return Get(key); }

            set { Set(key, value); }
        }

        public bool ContainsKey(string key)
        {
            return Headers.AllKeys.Contains(key, StringComparer.OrdinalIgnoreCase);
        }

        public virtual bool Remove(string key)
        {
            if (ContainsKey(key))
            {
                Headers.Remove(key);
                return true;
            }

            return false;
        }

        protected virtual void RemoveSilent(string header)
        {
            Headers.Remove(header);
        }

        protected virtual string[] Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            string[] values;
            if (!TryGetValue(key, out values))
            {
                throw new KeyNotFoundException(key);
            }

            return values;
        }

        protected void Set(string key, string[] value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            RemoveSilent(key);
            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    Add(key, value[i]);
                }
            }
        }

        public void Add(string key, string[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            foreach (var value in values)
            {
                Add(key, value);
            }
        }

        public virtual void Add(string key, string value)
        {
            Headers.Add(key, value);
        }

        public virtual bool TryGetValue(string key, out string[] value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                value = null;
                return false;
            }

            value = Headers.GetValues(key);
            return value != null;
        }

        public void Add(KeyValuePair<string, string[]> item)
        {
            Add(item.Key, item.Value);
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
                yield return new KeyValuePair<string, string[]>(Headers.GetKey(i), Headers.GetValues(i));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
