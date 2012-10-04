//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Collections.Generic
{
    internal static class DictionaryExtensions
    {
        internal static T Get<T>(this IDictionary<string, object> dictionary, string key, T fallback = default(T))
        {
            object value;
            return dictionary.TryGetValue(key, out value) ? (T)value : fallback;
        }
    }
}