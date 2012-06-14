using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace Katana.Server.HttpListenerWrapper
{
    internal class NameValueToDictionaryWrapper : IDictionary<string, string[]>
    {
        private NameValueCollection innerCollection;

        internal NameValueToDictionaryWrapper(NameValueCollection innerCollection)
        {
            Debug.Assert(innerCollection != null);
            this.innerCollection = innerCollection;
        }

        public void Add(string key, string[] values)
        {
            foreach (string value in values)
            {
                innerCollection.Add(key, value);
            }
        }

        public bool ContainsKey(string key)
        {
            return innerCollection.AllKeys.Contains(key, StringComparer.OrdinalIgnoreCase);
        }

        public ICollection<string> Keys
        {
            get { return innerCollection.AllKeys; }
        }

        public bool Remove(string key)
        {
            if (ContainsKey(key))
            {
                innerCollection.Remove(key);
                return true;
            }
            return false;
        }

        public bool TryGetValue(string key, out string[] value)
        {
            value = innerCollection.GetValues(key);
            return value == null;
        }

        public ICollection<string[]> Values
        {
            get { return this.Select(pair => pair.Value).ToList(); }
        }

        public string[] this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                string[] values = innerCollection.GetValues(key);
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
                Remove(key);
                if (value != null)
                {
                    foreach (string item in value)
                    {
                        innerCollection.Add(key, item);
                    }
                }
            }
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
                innerCollection.Add(item.Key, value);
            }
        }

        public void Clear()
        {
            innerCollection.Clear();
        }

        public bool Contains(KeyValuePair<string, string[]> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, string[]>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return innerCollection.AllKeys.Count(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<string, string[]> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            for (int i = 0; i < innerCollection.Count; i++)
            {
                yield return new KeyValuePair<string, string[]>(innerCollection.GetKey(i), innerCollection.GetValues(i));
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
