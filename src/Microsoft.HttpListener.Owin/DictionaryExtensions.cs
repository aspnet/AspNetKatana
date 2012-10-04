//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Collections.Generic
{
    internal static class DictionaryExtensions
    {
        internal static T Get<T>(this IDictionary<string, object> dictionary, string key)
        {
            object value;
            return dictionary.TryGetValue(key, out value) ? (T)value : default(T);
        }

        internal static T Get<T>(this IDictionary<string, object> dictionary, string subDictionaryKey, string key)
        {
            var subDictionary = dictionary.Get<IDictionary<string, object>>(subDictionaryKey);
            if (subDictionary == null)
            {
                return default(T);
            }

            object value;
            return dictionary.TryGetValue(key, out value) ? (T)value : default(T);
        }
    }
}