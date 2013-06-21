// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Owin.Infrastructure;

namespace Microsoft.Owin
{
    public class ReadableStringCollection : IReadableStringCollection
    {
        public ReadableStringCollection(IDictionary<string, string[]> store)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }

            Store = store;
        }

        private IDictionary<string, string[]> Store { get; set; }

        public string this[string key]
        {
            get { return Get(key); }
        }

        public string Get(string key)
        {
            return OwinHelpers.GetJoinedValue(Store, key);
        }

        public IList<string> GetValues(string key)
        {
            string[] values;
            Store.TryGetValue(key, out values);
            return values;
        }

        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            return Store.GetEnumerator();
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
