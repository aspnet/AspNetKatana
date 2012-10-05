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
        internal static T Get<T>(this IDictionary<string, object> dictionary, string key, T fallback = default(T))
        {
            object value;
            return dictionary.TryGetValue(key, out value) ? (T)value : fallback;
        }
    }
}