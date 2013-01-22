// <copyright file="Util.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
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

using System.Collections.Generic;
using System.IO;

namespace Katana.Performance.ReferenceApp
{
    public static class Util
    {
        public static IEnumerable<byte> AlphabetCRLF(int length)
        {
            while (true)
            {
                for (char ch = 'a'; ch != 'z' + 1; ++ch)
                {
                    if (length-- == 0)
                    {
                        yield break;
                    }
                    yield return (byte)ch;
                }
                if (length-- == 0)
                {
                    yield break;
                }
                yield return (byte)' ';
                for (char ch = 'A'; ch != 'Z' + 1; ++ch)
                {
                    if (length-- == 0)
                    {
                        yield break;
                    }
                    yield return (byte)ch;
                }
                if (length-- == 0)
                {
                    yield break;
                }
                yield return (byte)'\r';
                if (length-- == 0)
                {
                    yield break;
                }
                yield return (byte)'\n';
            }
        }

        public static T Get<T>(IDictionary<string, object> env, string key)
        {
            object value;
            return env.TryGetValue(key, out value) ? (T)value : default(T);
        }

        public static Stream ResponseBody(IDictionary<string, object> env)
        {
            return Get<Stream>(env, "owin.ResponseBody");
        }

        public static IDictionary<string, string[]> ResponseHeaders(IDictionary<string, object> env)
        {
            return Get<IDictionary<string, string[]>>(env, "owin.ResponseHeaders");
        }

        public static string RequestPath(IDictionary<string, object> env)
        {
            return Get<string>(env, "owin.RequestPath");
        }
    }
}
