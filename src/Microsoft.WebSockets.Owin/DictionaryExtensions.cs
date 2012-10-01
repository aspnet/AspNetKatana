using System.Collections.Generic;

namespace System.Collections.Generic
{
    static class DictionaryExtensions
    {
        public static T Get<T>(this IDictionary<string, object> dictionary, string key, T fallback = default(T))
        {
            object value;
            return dictionary.TryGetValue(key, out value) ? (T)value : fallback;
        }
    }
}