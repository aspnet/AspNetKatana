using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace Katana.WebApi.CallHeaders
{
    public abstract partial class MessageHeadersWrapper : IDictionary<string, string[]>
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
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            MessageHeaders.Clear();
            if (ContentHeaders != null)
                ContentHeaders.Clear();
        }

        public bool Contains(KeyValuePair<string, string[]> item)
        {
            var pair = new KeyValuePair<string, IEnumerable<string>>(item.Key, item.Value);

            return (!IsContentHeader(item.Key) && MessageHeaders.Contains(pair, new KvComparer())) ||
                (ContentHeaders != null && !IsMessageHeader(item.Key) && ContentHeaders.Contains(pair, new KvComparer()));
        }

        class KvComparer : IEqualityComparer<KeyValuePair<string, IEnumerable<string>>>
        {
            public bool Equals(KeyValuePair<string, IEnumerable<string>> x, KeyValuePair<string, IEnumerable<string>> y)
            {
                return string.Equals(x.Key, y.Key, StringComparison.OrdinalIgnoreCase) &&
                    x.Value.Count() == y.Value.Count() &&
                    x.Value.Zip(y.Value, string.Equals).All(z => z);
            }

            public int GetHashCode(KeyValuePair<string, IEnumerable<string>> obj)
            {
                throw new NotImplementedException();
            }
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
            return Contains(item) && Remove(item.Key);
        }

        public int Count
        {
            get { return ContentHeaders != null ? MessageHeaders.Count() + ContentHeaders.Count() : MessageHeaders.Count(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool ContainsKey(string key)
        {
            return (!IsContentHeader(key) && MessageHeaders.Contains(key)) ||
                (ContentHeaders != null && !IsMessageHeader(key) && ContentHeaders.Contains(key));

        }

        public void Add(string key, string[] value)
        {
            if (!MessageHeaders.TryAddWithoutValidation(key, value))
            {
                if (ContentHeaders == null || !ContentHeaders.TryAddWithoutValidation(key, value))
                {
                    throw new InvalidOperationException("Unable to add header");
                }
            }
        }

        public bool Remove(string key)
        {
            var removed = false;
            if (!IsContentHeader(key))
            {
                removed |= MessageHeaders.Remove(key);
            }
            if (ContentHeaders != null && !IsMessageHeader(key))
            {
                removed |= ContentHeaders.Remove(key);
            }
            return removed;
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
            set
            {
                Remove(key);
                Add(key, value);
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                var keys = MessageHeaders.Select(kv => kv.Key);
                if (ContentHeaders != null)
                    keys = keys.Concat(ContentHeaders.Select(kv => kv.Key)).Distinct();
                return keys.ToList();
            }
        }

        public ICollection<string[]> Values
        {
            get
            {
                var keys = MessageHeaders.Select(kv => kv.Value.ToArray());
                if (ContentHeaders != null)
                    keys = keys.Concat(ContentHeaders.Select(kv => kv.Value.ToArray()));
                return keys.ToList();
            }
        }
    }
}
