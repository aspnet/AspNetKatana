// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin.Infrastructure;

namespace Microsoft.Owin
{
    /// <summary>
    /// A wrapper for owin.RequestHeaders and owin.ResponseHeaders
    /// </summary>
    public class HeaderDictionary : IHeaderDictionary
    {
        /// <summary>
        /// Create a new wrapper
        /// </summary>
        /// <param name="store"></param>
        public HeaderDictionary(IDictionary<string, string[]> store)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }

            Store = store;
        }

        private IDictionary<string, string[]> Store { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ICollection<string> Keys
        {
            get { return Store.Keys; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICollection<string[]> Values
        {
            get { return Store.Values; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Count
        {
            get { return Store.Count; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsReadOnly
        {
            get { return Store.IsReadOnly; }
        }

        /// <summary>
        /// Get or set the associated header value in the collection.  Multiple values will be merged.
        /// Returns null if the key is not present.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get { return Get(key); }
            set { Set(key, value); }
        }

        /// <summary>
        /// Throws KeyNotFoundException if the key is not present.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string[] IDictionary<string, string[]>.this[string key]
        {
            get { return Store[key]; }
            set { Store[key] = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            return Store.GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Get the associated value from the collection.  Multiple values will be merged.
        /// Returns null if the key is not present.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            return OwinHelpers.GetHeader(Store, key);
        }

        /// <summary>
        /// Get the associated values from the collection in their original format.
        /// Returns null if the key is not present.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IList<string> GetValues(string key)
        {
            return OwinHelpers.GetHeaderUnmodified(Store, key);
        }

        /// <summary>
        /// Parses out comma separated headers into individual values.  Quoted values will not be coma split, and the quotes will be removed.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IList<string> GetCommaSeparatedValues(string key)
        {
            IEnumerable<string> values = OwinHelpers.GetHeaderSplit(Store, key);
            return values == null ? null : values.ToList();
        }

        // TODO: Review which of these overloads joins, quotes, etc.

        /// <summary>
        /// Add a new value. Appends to the header if already present
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Append(string key, string value)
        {
            OwinHelpers.AppendHeader(Store, key, value);
        }

        /// <summary>
        /// Add new values. Each item remains a separate array entry.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public void AppendValues(string key, params string[] values)
        {
            OwinHelpers.AppendHeaderUnmodified(Store, key, values);
        }

        /// <summary>
        /// Quotes any values containing comas, and then coma joins all of the values with any existing values.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public void AppendCommaSeparatedValues(string key, params string[] values)
        {
            OwinHelpers.AppendHeaderJoined(Store, key, values);
        }

        /// <summary>
        /// Sets a specific header value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value)
        {
            OwinHelpers.SetHeader(Store, key, value);
        }

        /// <summary>
        /// Sets the specified header values without modification
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public void SetValues(string key, params string[] values)
        {
            OwinHelpers.SetHeaderUnmodified(Store, key, values);
        }

        /// <summary>
        /// Quotes any values containing comas, and then coma joins all of the values.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public void SetCommaSeparatedValues(string key, params string[] values)
        {
            OwinHelpers.SetHeaderJoined(Store, key, values);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, string[] value)
        {
            Store.Add(key, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return Store.ContainsKey(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            return Store.Remove(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, out string[] value)
        {
            return Store.TryGetValue(key, out value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Add(KeyValuePair<string, string[]> item)
        {
            Store.Add(item);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            Store.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<string, string[]> item)
        {
            return Store.Contains(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(KeyValuePair<string, string[]>[] array, int arrayIndex)
        {
            Store.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(KeyValuePair<string, string[]> item)
        {
            return Store.Remove(item);
        }
    }
}
