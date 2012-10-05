// Copyright 2011-2012 Katana contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace System.Collections.Generic
{
    internal static class DictionaryExtensions
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
