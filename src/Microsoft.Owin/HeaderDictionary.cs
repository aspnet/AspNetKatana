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

        public new string this[string key]
        {
            get { return Get(key); }
            set { Set(key, value); }
        }

        #region Implementation of IEnumerable

        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            return Store.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IReadableStringCollection

        public string Get(string key)
        {
            return OwinHelpers.GetHeader(Store, key);
        }

        public IList<string> GetValues(string key)
        {
            return OwinHelpers.GetHeaderUnmodified(Store, key);
        }

        #endregion

        #region Implementation of IHeaderCollection

        public IList<string> GetCommaSeparatedValues(string key)
        {
            return OwinHelpers.GetHeaderSplit(Store, key).ToList();
        }

        // TODO: Review which of these overloads joins, quotes, etc.
        public void Append(string key, string value)
        {
            OwinHelpers.AddHeaderJoined(Store, key, value);
        }

        public void AppendValues(string key, params string[] values)
        {
            OwinHelpers.AddHeaderUnmodified(Store, key, values);
        }

        public void AppendCommaSeparatedValues(string key, params string[] values)
        {
            OwinHelpers.AddHeaderJoined(Store, key, values);
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

        #endregion

        #region Implementation of IDictionary

        public void Add(string key, string[] value)
        {
            Store.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return Store.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return Store.Keys; }
        }

        public bool Remove(string key)
        {
            return Store.Remove(key);
        }

        public bool TryGetValue(string key, out string[] value)
        {
            return Store.TryGetValue(key, out value);
        }

        public ICollection<string[]> Values
        {
            get { return Store.Values; }
        }

        string[] IDictionary<string, string[]>.this[string key]
        {
            get { return Store[key]; }
            set { Store[key] = value; }
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

        public int Count
        {
            get { return Store.Count; }
        }

        public bool IsReadOnly
        {
            get { return Store.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<string, string[]> item)
        {
            return Store.Remove(item);
        }

        #endregion
    }
}
