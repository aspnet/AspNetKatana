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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.AspNet.Owin.CallHeaders
{
    public class AspNetRequestHeaders : IDictionary<string, string[]>, IEnumerable<KeyValuePair<string, string>>
    {
        ICollection<string> IDictionary<string, string[]>.Keys
        {
            get { throw new NotImplementedException(); }
        }

        ICollection<string[]> IDictionary<string, string[]>.Values
        {
            get { throw new NotImplementedException(); }
        }

        int ICollection<KeyValuePair<string, string[]>>.Count
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<KeyValuePair<string, string[]>>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        string[] IDictionary<string, string[]>.this[string key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public static IDictionary<string, string[]> Create(HttpRequestBase httpRequest)
        {
            // PERF: this method will return an IDictionary facade to enable two things...
            //   direct enumeration from original headers if only GetEnumerator is called,
            //   readonly responses for a few operations from original namevaluecollection,
            //   just-in-time creation and pass-through to real Dictionary for other calls
            return httpRequest.Headers.AllKeys.ToDictionary(
                key => key,
                key => (string[])httpRequest.Headers.GetValues(key),
                StringComparer.OrdinalIgnoreCase);
        }

        void IDictionary<string, string[]>.Add(string key, string[] value)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, string[]>.ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, string[]>.Remove(string key)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, string[]>.TryGetValue(string key, out string[] value)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, string[]>>.Add(KeyValuePair<string, string[]> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, string[]>>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, string[]>>.Contains(KeyValuePair<string, string[]> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, string[]>>.CopyTo(KeyValuePair<string, string[]>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, string[]>>.Remove(KeyValuePair<string, string[]> item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<string, string[]>> IEnumerable<KeyValuePair<string, string[]>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
