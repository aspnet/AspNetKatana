//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Katana.Server.HttpListenerWrapper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// This wraps HttpListenerRequest's WebHeaderCollection (NameValueCollection) and adapts it to 
    /// the OWIN required IDictionary surface area. It remains fully mutable, but you will be subject 
    /// to the header validations performed by the underlying collection.
    /// </summary>
    internal class NameValueToDictionaryWrapper : IDictionary<string, string[]>
    {
        private NameValueCollection innerCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="NameValueToDictionaryWrapper"/> class
        /// </summary>
        /// <param name="innerCollection">The WebHeaderCollection from HttpListenerRequest.Headers</param>
        internal NameValueToDictionaryWrapper(NameValueCollection innerCollection)
        {
            Contract.Requires(innerCollection != null);
            this.innerCollection = innerCollection;
        }

        public ICollection<string> Keys
        {
            get { return this.innerCollection.AllKeys; }
        }

        public ICollection<string[]> Values
        {
            get { return this.Select(pair => pair.Value).ToList(); }
        }

        public int Count
        {
            get { return this.innerCollection.AllKeys.Count(); }
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

                string[] values = this.innerCollection.GetValues(key);
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

                this.Remove(key);
                if (value != null)
                {
                    foreach (string item in value)
                    {
                        this.innerCollection.Add(key, item);
                    }
                }
            }
        }

        public void Add(string key, string[] values)
        {
            foreach (string value in values)
            {
                this.innerCollection.Add(key, value);
            }
        }

        public bool ContainsKey(string key)
        {
            return this.innerCollection.AllKeys.Contains(key, StringComparer.OrdinalIgnoreCase);
        }

        public bool Remove(string key)
        {
            if (this.ContainsKey(key))
            {
                this.innerCollection.Remove(key);
                return true;
            }

            return false;
        }

        public bool TryGetValue(string key, out string[] value)
        {
            value = this.innerCollection.GetValues(key);
            return value != null;
        }

        public void Add(KeyValuePair<string, string[]> item)
        {
            if (item.Key == null)
            {
                throw new ArgumentNullException("item.Key");
            }

            if (item.Value == null)
            {
                throw new ArgumentNullException("item.Value");
            }

            foreach (string value in item.Value)
            {
                this.innerCollection.Add(item.Key, value);
            }
        }

        public void Clear()
        {
            this.innerCollection.Clear();
        }

        public bool Contains(KeyValuePair<string, string[]> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, string[]>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, string[]> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            for (int i = 0; i < this.innerCollection.Count; i++)
            {
                yield return new KeyValuePair<string, string[]>(this.innerCollection.GetKey(i), this.innerCollection.GetValues(i));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
