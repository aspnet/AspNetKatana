using System.Collections.Generic;

namespace Microsoft.AspNet.WebApi.Owin.Tests
{
    static class DictionaryExtensions
    {
        public static TValue Get<TValue>(
            this IDictionary<object, object> dictionary,
            object key)
        {
            return dictionary.Get<object, TValue>(key);
        }

        public static TValue Get<TValue>(
            this IDictionary<string, object> dictionary,
            string key)
        {
            return dictionary.Get<string, TValue>(key);
        }

        public static TValue Get<TKey, TValue>(
            this IDictionary<TKey, object> dictionary,
            TKey key)
        {
            object value;
            return dictionary.TryGetValue(key, out value) ? (TValue)value : default(TValue);
        }

        public static IDictionary<TKey, TValue> Set<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue value)
        {
            dictionary[key] = value;
            return dictionary;
        }

        public static IDictionary<TKey, TValue[]> Set<TKey, TValue>(
            this IDictionary<TKey, TValue[]> dictionary,
            TKey key,
            TValue value)
        {
            dictionary[key] = new[] { value };
            return dictionary;
        }

        public static IDictionary<TKey, IEnumerable<TValue>> Set<TKey, TValue>(
            this IDictionary<TKey, IEnumerable<TValue>> dictionary,
            TKey key,
            TValue value)
        {
            dictionary[key] = new[] { value };
            return dictionary;
        }
    }
}