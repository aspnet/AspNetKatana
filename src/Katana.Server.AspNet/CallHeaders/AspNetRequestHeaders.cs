using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Katana.Server.AspNet.CallHeaders
{
    public class AspNetRequestHeaders : IDictionary<string,IEnumerable<string>>
    {
        public static IDictionary<string, IEnumerable<string>> Create(HttpRequestBase httpRequest)
        {
            // PERF: this method will return an IDictionary facade to enable two things...
            //   direct enumeration from original headers if only GetEnumerator is called,
            //   readonly responses for a few operations from original namevaluecollection,
            //   just-in-time creation and pass-through to real Dictionary for other calls
            return httpRequest.Headers.AllKeys.ToDictionary(
                key => key, 
                key => (IEnumerable<string>)httpRequest.Headers.GetValues(key),
                StringComparer.OrdinalIgnoreCase);
        }

        void IDictionary<string, IEnumerable<string>>.Add(string key, IEnumerable<string> value)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, IEnumerable<string>>.ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        ICollection<string> IDictionary<string, IEnumerable<string>>.Keys
        {
            get { throw new NotImplementedException(); }
        }

        bool IDictionary<string, IEnumerable<string>>.Remove(string key)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, IEnumerable<string>>.TryGetValue(string key, out IEnumerable<string> value)
        {
            throw new NotImplementedException();
        }

        ICollection<IEnumerable<string>> IDictionary<string, IEnumerable<string>>.Values
        {
            get { throw new NotImplementedException(); }
        }

        IEnumerable<string> IDictionary<string, IEnumerable<string>>.this[string key]
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

        void ICollection<KeyValuePair<string, IEnumerable<string>>>.Add(KeyValuePair<string, IEnumerable<string>> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, IEnumerable<string>>>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, IEnumerable<string>>>.Contains(KeyValuePair<string, IEnumerable<string>> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, IEnumerable<string>>>.CopyTo(KeyValuePair<string, IEnumerable<string>>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        int ICollection<KeyValuePair<string, IEnumerable<string>>>.Count
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<KeyValuePair<string, IEnumerable<string>>>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<KeyValuePair<string, IEnumerable<string>>>.Remove(KeyValuePair<string, IEnumerable<string>> item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<string, IEnumerable<string>>> IEnumerable<KeyValuePair<string, IEnumerable<string>>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
