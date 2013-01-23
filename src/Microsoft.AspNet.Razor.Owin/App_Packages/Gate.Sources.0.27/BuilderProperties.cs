using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gate
{
    public struct BuilderProperties
    {
        IDictionary<string, object> _properties;

        public BuilderProperties(IDictionary<string, object> properties)
        {
            _properties = properties;
        }

        // Retrieves the value if present, or returns the default (null) otherwise.
        public T Get<T>(string key)
        {
            object value;
            return _properties.TryGetValue(key, out value) ? (T)value : default(T);
        }

        // Sets the value if non-null, or removes it otherwise
        public BuilderProperties Set<T>(string key, T value)
        {
            if (object.Equals(value, default(T)))
            {
                _properties.Remove(key);
            }
            else
            {
                _properties[key] = value;
            }
            return this;
        }

        public string Version
        {
            get { return Get<string>(OwinConstants.Version); }
        }

        /// <summary>
        /// "host.TraceOutput" A TextWriter that directs trace or logger output to an appropriate place for the host
        /// </summary>
        public TextWriter TraceOutput
        {
            get { return Get<TextWriter>(OwinConstants.TraceOutput); }
            set { Set<TextWriter>(OwinConstants.TraceOutput, value); }
        }
    }
}
