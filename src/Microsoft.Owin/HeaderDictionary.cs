// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin.Infrastructure;

namespace Microsoft.Owin
{
    public class HeaderDictionary : IHeaderDictionary
    {
        public HeaderDictionary(IDictionary<string, string[]> store)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }

            Store = store;
        }

        private IDictionary<string, string[]> Store { get; set; }

        public ICollection<string> Keys
        {
            get { return Store.Keys; }
        }

        public ICollection<string[]> Values
        {
            get { return Store.Values; }
        }

        public int Count
        {
            get { return Store.Count; }
        }

        public bool IsReadOnly
        {
            get { return Store.IsReadOnly; }
        }

        public string this[string key]
        {
            get { return Get(key); }
            set { Set(key, value); }
        }

        string[] IDictionary<string, string[]>.this[string key]
        {
            get { return Store[key]; }
            set { Store[key] = value; }
        }

        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            return Store.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string Get(string key)
        {
            return OwinHelpers.GetHeader(Store, key);
        }

        public IList<string> GetValues(string key)
        {
            return OwinHelpers.GetHeaderUnmodified(Store, key);
        }

        public IList<string> GetCommaSeparatedValues(string key)
        {
            IEnumerable<string> values = OwinHelpers.GetHeaderSplit(Store, key);
            return values == null ? null : values.ToList();
        }

        // TODO: Review which of these overloads joins, quotes, etc.
        public void Append(string key, string value)
        {
            OwinHelpers.AppendHeader(Store, key, value);
        }

        public void AppendValues(string key, params string[] values)
        {
            OwinHelpers.AppendHeaderUnmodified(Store, key, values);
        }

        public void AppendCommaSeparatedValues(string key, params string[] values)
        {
            OwinHelpers.AppendHeaderJoined(Store, key, values);
        }

        public void Set(string key, string value)
        {
            OwinHelpers.SetHeader(Store, key, value);
        }

        public void SetValues(string key, params string[] values)
        {
            OwinHelpers.SetHeaderUnmodified(Store, key, values);
        }

        public void SetCommaSeparatedValues(string key, params string[] values)
        {
            OwinHelpers.SetHeaderJoined(Store, key, values);
        }

        public void Add(string key, string[] value)
        {
            Store.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return Store.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return Store.Remove(key);
        }

        public bool TryGetValue(string key, out string[] value)
        {
            return Store.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, string[]> item)
        {
            Store.Add(item);
        }

        public void Clear()
        {
            Store.Clear();
        }

        public bool Contains(KeyValuePair<string, string[]> item)
        {
            return Store.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, string[]>[] array, int arrayIndex)
        {
            Store.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, string[]> item)
        {
            return Store.Remove(item);
        }
    }
}
