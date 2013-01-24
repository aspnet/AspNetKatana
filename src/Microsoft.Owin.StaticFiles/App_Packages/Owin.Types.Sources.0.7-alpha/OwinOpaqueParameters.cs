using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Owin.Types
{
#region OwinOpaqueParameters

    internal partial struct OwinOpaqueParameters
    {
        public static OwinOpaqueParameters Create()
        {
            return new OwinOpaqueParameters(new ConcurrentDictionary<string, object>(StringComparer.Ordinal));
        }
    }
#endregion

#region OwinOpaqueParameters.Generated

    internal partial struct OwinOpaqueParameters
    {
        private readonly IDictionary<string, object> _dictionary;

        public OwinOpaqueParameters(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        public IDictionary<string, object> Dictionary
        {
            get { return _dictionary; }
        }

#region Value-type equality
        public bool Equals(OwinOpaqueParameters other)
        {
            return Equals(_dictionary, other._dictionary);
        }

        public override bool Equals(object obj)
        {
            return obj is OwinOpaqueParameters && Equals((OwinOpaqueParameters)obj);
        }

        public override int GetHashCode()
        {
            return (_dictionary != null ? _dictionary.GetHashCode() : 0);
        }

        public static bool operator ==(OwinOpaqueParameters left, OwinOpaqueParameters right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OwinOpaqueParameters left, OwinOpaqueParameters right)
        {
            return !left.Equals(right);
        }
#endregion

        public T Get<T>(string key)
        {
            object value;
            return _dictionary.TryGetValue(key, out value) ? (T)value : default(T);
        }

        public OwinOpaqueParameters Set(string key, object value)
        {
            _dictionary[key] = value;
            return this;
        }

    }
#endregion

}
