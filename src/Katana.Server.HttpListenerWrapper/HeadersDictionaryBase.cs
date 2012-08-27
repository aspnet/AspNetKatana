using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Katana.Server.HttpListenerWrapper
{
    internal abstract class HeadersDictionaryBase : IDictionary<string, string[]>
    {
        protected WebHeaderCollection Headers { get; private set; }

        protected HeadersDictionaryBase(WebHeaderCollection headers)
        {
            this.Headers = headers;
        }

        public ICollection<string> Keys
        {
            get { return this.Headers.AllKeys; }
        }

        public ICollection<string[]> Values
        {
            get { return this.Select(pair => pair.Value).ToList(); }
        }

        public int Count
        {
            get { return this.Headers.AllKeys.Count(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool ContainsKey(string key)
        {
            return this.Headers.AllKeys.Contains(key, StringComparer.OrdinalIgnoreCase);
        }

        public virtual bool Remove(string key)
        {
            if (this.ContainsKey(key))
            {
                this.Headers.Remove(key);
                return true;
            }

            return false;
        }

        public string[] this[string key]
        {
            get
            {
                return Get(key);
            }

            set
            {
                Set(key, value);
            }
        }

        protected string[] Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            string[] values = this.Headers.GetValues(key);
            if (values == null)
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

            this.Remove(key);
            if (value != null)
            {
                foreach (string item in value)
                {
                    this.Add(key, item);
                }
            }
        }

        public void Add(string key, string[] values)
        {
            foreach (string value in values)
            {
                this.Add(key, value);
            }
        }

        public virtual void Add(string key, string value)
        {
            this.Headers.Add(key, value);
        }

        public bool TryGetValue(string key, out string[] value)
        {
            value = this.Headers.GetValues(key);
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
                this.Headers.Add(item.Key, value);
            }
        }

        public void Clear()
        {
            this.Headers.Clear();
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
            for (int i = 0; i < this.Headers.Count; i++)
            {
                yield return new KeyValuePair<string, string[]>(this.Headers.GetKey(i), this.Headers.GetValues(i));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
