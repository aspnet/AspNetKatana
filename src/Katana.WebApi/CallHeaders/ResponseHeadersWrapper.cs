using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Katana.WebApi.CallHeaders
{
    public class ResponseHeadersWrapper : MessageHeadersWrapper
    {
        private readonly HttpResponseMessage _message;

        public ResponseHeadersWrapper(HttpResponseMessage message)
        {
            _message = message;
        }

        protected override HttpHeaders MessageHeaders
        {
            get { return _message.Headers; }
        }

        protected override HttpHeaders ContentHeaders
        {
            get { return _message.Content != null ? _message.Content.Headers : null; }
        }



        public void Add(KeyValuePair<string, string[]> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, string[]> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, string[]>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, string[]> item)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public void Add(string key, string[] value)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out string[] value)
        {
            throw new NotImplementedException();
        }

        public string[] this[string key]
        {
            get
            {
                IEnumerable<string> value;
                if (_message.Headers.TryGetValues(key, out value))
                {
                    return value.ToArray();
                }
                if (_message.Content != null && _message.Content.Headers.TryGetValues(key, out value))
                {
                    return value.ToArray();
                }
                throw new KeyNotFoundException();
            }
            set { throw new NotImplementedException(); }
        }

        public ICollection<string> Keys
        {
            get { throw new NotImplementedException(); }
        }

        public ICollection<string[]> Values
        {
            get { throw new NotImplementedException(); }
        }
    }
}
