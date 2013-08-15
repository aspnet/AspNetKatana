// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Owin.Host.HttpListener.RequestProcessing
{
    internal sealed partial class CallEnvironment : IDictionary<string, object>
    {
        private static readonly IDictionary<string, object> WeakNilEnvironment = new NilDictionary();

        private readonly IPropertySource _propertySource;
        private IDictionary<string, object> _extra = WeakNilEnvironment;

        internal CallEnvironment(IPropertySource propertySource)
        {
            _propertySource = propertySource;
        }

        internal IDictionary<string, object> Extra
        {
            get { return _extra; }
        }

        private IDictionary<string, object> StrongExtra
        {
            get
            {
                if (_extra == WeakNilEnvironment)
                {
                    _extra = new Dictionary<string, object>();
                }
                return _extra;
            }
        }

        internal bool IsExtraDictionaryCreated
        {
            get { return _extra != WeakNilEnvironment; }
        }

        public object this[string key]
        {
            get
            {
                object value;
                return PropertiesTryGetValue(key, out value) ? value : Extra[key];
            }
            set
            {
                if (!PropertiesTrySetValue(key, value))
                {
                    StrongExtra[key] = value;
                }
            }
        }

        public void Add(string key, object value)
        {
            if (!PropertiesTrySetValue(key, value))
            {
                StrongExtra.Add(key, value);
            }
        }

        public bool ContainsKey(string key)
        {
            return PropertiesContainsKey(key) || Extra.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return PropertiesKeys().Concat(Extra.Keys).ToArray(); }
        }

        public bool Remove(string key)
        {
            // Although this is a mutating operation, Extra is used instead of StrongExtra,
            // because if a real dictionary has not been allocated the default behavior of the
            // nil dictionary is perfectly fine.
            return PropertiesTryRemove(key) || Extra.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return PropertiesTryGetValue(key, out value) || Extra.TryGetValue(key, out value);
        }

        public ICollection<object> Values
        {
            get { return PropertiesValues().Concat(Extra.Values).ToArray(); }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            ((IDictionary<string, object>)this).Add(item.Key, item.Value);
        }

        public void Clear()
        {
            foreach (var key in PropertiesKeys())
            {
                PropertiesTryRemove(key);
            }
            Extra.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            object value;
            return ((IDictionary<string, object>)this).TryGetValue(item.Key, out value) && Object.Equals(value, item.Value);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            PropertiesEnumerable().Concat(Extra).ToArray().CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return PropertiesKeys().Count() + Extra.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>)this).Contains(item) &&
                ((IDictionary<string, object>)this).Remove(item.Key);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return PropertiesEnumerable().Concat(Extra).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<string, object>)this).GetEnumerator();
        }
    }
}
