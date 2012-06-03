using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Katana.Server.AspNet.CallEnvironment
{
    public class NilDictionary : IDictionary<string, object>
    {
        private static readonly string[] EmptyKeys = new string[0];
        private static readonly object[] EmptyValues = new object[0];
        private static readonly IEnumerable<KeyValuePair<string, object>> EmptyKeyValuePairs = Enumerable.Empty<KeyValuePair<string, object>>();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return EmptyKeyValuePairs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return EmptyKeyValuePairs.GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return false;
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return false;
        }

        public int Count
        {
            get { return 0; }
        }

        public bool IsReadOnly
        {
            get { return false;}
        }

        public bool ContainsKey(string key)
        {
            return false;
        }

        public void Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            return false;
        }

        public bool TryGetValue(string key, out object value)
        {
            value = null;
            return false;
        }

        public object this[string key]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public ICollection<string> Keys
        {
            get { return EmptyKeys; }
        }

        public ICollection<object> Values
        {
            get { return EmptyValues; }
        }
    }
}