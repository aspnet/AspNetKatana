using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace Katana.WebApi.CallHeaders
{
    public abstract class MessageHeadersWrapper : IDictionary<string, string[]>
    {
        protected abstract HttpHeaders MessageHeaders { get; }
        protected abstract HttpHeaders ContentHeaders { get; }

        IEnumerable<KeyValuePair<string, string[]>> GetHeaders()
        {
            var headers = MessageHeaders.Select(Adapt);
            return ContentHeaders != null ? headers.Concat(ContentHeaders.Select(Adapt)) : headers;
        }

        private static KeyValuePair<string, string[]> Adapt(KeyValuePair<string, IEnumerable<string>> kv)
        {
            return new KeyValuePair<string, string[]>(kv.Key, kv.Value.ToArray());
        }

        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            return GetHeaders().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, string[]> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, string[]> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, string[]>[] array, int arrayIndex)
        {
            var index = arrayIndex;
            foreach (var header in GetHeaders())
            {
                array[index++] = header;
            }
        }

        public bool Remove(KeyValuePair<string, string[]> item)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return ContentHeaders != null ? MessageHeaders.Count() + ContentHeaders.Count() : MessageHeaders.Count(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public void Add(string key, string[] value)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out string[] value)
        {
            IEnumerable<string> values;
            if (MessageHeaders.TryGetValues(key, out values))
            {
                value = values.ToArray();
                return true;
            }
            if (ContentHeaders != null && ContentHeaders.TryGetValues(key, out values))
            {
                value = values.ToArray();
                return true;
            }
            value = null;
            return false;
        }

        public string[] this[string key]
        {
            get
            {
                string[] value;
                if (TryGetValue(key, out value))
                    return value;
                throw new KeyNotFoundException();
            }
            set { throw new NotImplementedException(); }
        }

        public ICollection<string> Keys
        {
            get { throw new NotImplementedException(); }
        }

        public ICollection<string[]> Values
        {
            get { throw new NotImplementedException(); }
        }
    }
}
