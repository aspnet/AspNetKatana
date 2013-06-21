// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Owin
{
    public class RequestCookieCollection : IEnumerable<KeyValuePair<string, string>>
    {
        public RequestCookieCollection(IDictionary<string, string> store)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }

            Store = store;
        }

        private IDictionary<string, string> Store { get; set; }

        // Returns null rather than throwing KeyNotFoundException
        public string this[string key]
        {
            get
            {
                string value;
                Store.TryGetValue(key, out value);
                return value;
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return Store.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
