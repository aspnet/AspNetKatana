using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Owin.Types
{
#region OwinOpaque.Generated

    internal partial struct OwinOpaque
    {
        private readonly IDictionary<string, object> _dictionary;

        public OwinOpaque(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        public IDictionary<string, object> Dictionary
        {
            get { return _dictionary; }
        }

#region Value-type equality
        public bool Equals(OwinOpaque other)
        {
            return Equals(_dictionary, other._dictionary);
        }

        public override bool Equals(object obj)
        {
            return obj is OwinOpaque && Equals((OwinOpaque)obj);
        }

        public override int GetHashCode()
        {
            return (_dictionary != null ? _dictionary.GetHashCode() : 0);
        }

        public static bool operator ==(OwinOpaque left, OwinOpaque right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OwinOpaque left, OwinOpaque right)
        {
            return !left.Equals(right);
        }
#endregion

        public T Get<T>(string key)
        {
            object value;
            return _dictionary.TryGetValue(key, out value) ? (T)value : default(T);
        }

        public OwinOpaque Set(string key, object value)
        {
            _dictionary[key] = value;
            return this;
        }

    }
#endregion

#region OwinOpaque.Spec-Opaque

    internal partial struct OwinOpaque
    {
        public string Version
        {
            get { return Get<string>(OwinConstants.Opaque.Version); }
            set { Set(OwinConstants.Opaque.Version, value); }
        }

        public CancellationToken CallCancelled
        {
            get { return Get<CancellationToken>(OwinConstants.Opaque.CallCancelled); }
            set { Set(OwinConstants.Opaque.CallCancelled, value); }
        }

        public Stream Stream
        {
            get { return Get<Stream>(OwinConstants.Opaque.Stream); }
            set { Set(OwinConstants.Opaque.Stream, value); }
        }
    }
#endregion

}
