using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Owin;

namespace Gate
{
    using BodyAction = Func<
        Func< //next
            ArraySegment<byte>, // data
            Action, // continuation
            bool>, // continuation was or will be invoked
        Action<Exception>, //error
        Action, //complete
        Action>; //cancel

    /// <summary>
    /// Utility class providing strongly-typed get/set access to environment properties 
    /// defined by the OWIN spec.
    /// </summary>
    internal class Environment : IDictionary<string, object>
    {
        public const string RequestMethodKey = OwinConstants.RequestMethod;
        public const string RequestPathBaseKey = OwinConstants.RequestPathBase;
        public const string RequestPathKey = OwinConstants.RequestPath;
        public const string RequestQueryStringKey = OwinConstants.RequestQueryString;
        public const string RequestBodyKey = OwinConstants.RequestBody;
        public const string RequestHeadersKey = OwinConstants.RequestHeaders;
        public const string RequestSchemeKey = OwinConstants.RequestScheme;
        public const string VersionKey = OwinConstants.Version;

        public IDictionary<string, object> Env { get; set; }

        protected T Get<T>(string name)
        {
            object value;
            return TryGetValue(name, out value) ? (T)value : default(T);
        }

        public Environment()
        {
            Env = new Dictionary<string, object>();
        }

        public Environment(IDictionary<string, object> env)
        {
            Env = env;
        }

        /// <summary>
        /// "owin.Version" The string "1.0" indicating OWIN version 1.0. 
        /// </summary>
        public string Version
        {
            get { return Get<string>(VersionKey); }
            set { this[VersionKey] = value; }
        }

        /// <summary>
        /// "owin.RequestMethod" A string containing the HTTP request method of the request (e.g., "GET", "POST"). 
        /// </summary>
        public string Method
        {
            get { return Get<string>(RequestMethodKey); }
            set { this[RequestMethodKey] = value; }
        }

        /// <summary>
        /// "owin.RequestHeaders" An instance of IDictionary&lt;string, string&gt; which represents the HTTP headers present in the request (the request header dictionary).
        /// </summary>
        public IDictionary<string, IEnumerable<string>> Headers
        {
            get { return Get<IDictionary<string, IEnumerable<string>>>(RequestHeadersKey); }
            set { this[RequestHeadersKey] = value; }
        }

        /// <summary>
        /// "owin.RequestPathBase" A string containing the portion of the request path corresponding to the "root" of the application delegate. The value may be an empty string.  
        /// </summary>
        public string PathBase
        {
            get { return Get<string>(RequestPathBaseKey); }
            set { this[RequestPathBaseKey] = value; }
        }

        /// <summary>
        /// "owin.RequestPath" A string containing the request path. The path must be relative to the "root" of the application delegate. 
        /// </summary>
        public string Path
        {
            get { return Get<string>(RequestPathKey); }
            set { this[RequestPathKey] = value; }
        }

        /// <summary>
        /// "owin.RequestScheme" A string containing the URI scheme used for the request (e.g., "http", "https").  
        /// </summary>
        public string Scheme
        {
            get { return Get<string>(RequestSchemeKey); }
            set { this[RequestSchemeKey] = value; }
        }

        /// <summary>
        /// "owin.RequestBody" An instance of the body delegate representing the body of the request. May be null.
        /// </summary>
        public BodyAction BodyAction
        {
            get
            {
                object body;
                if (!TryGetValue(RequestBodyKey, out body))
                    return null;

                if (body is BodyDelegate)
                    return ToAction((BodyDelegate)body);

                return (BodyAction)body;
            }
            set { this[RequestBodyKey] = value; }
        }
        static BodyAction ToAction(BodyDelegate body)
        {
            return (next, error, complete) =>
            {
                var cts = new CancellationTokenSource();
                body(
                    data => next(data, null),
                    _ => false,
                    ex =>
                    {
                        if (ex == null) complete();
                        else error(ex);
                    }, cts.Token);
                return () => cts.Cancel();
            };
        }
        /// <summary>
        /// "owin.RequestBody" An instance of the body delegate representing the body of the request. May be null.
        /// </summary>
        public BodyDelegate BodyDelegate
        {
            get
            {
                object body;
                if (!TryGetValue(RequestBodyKey, out body))
                    return null;

                if (body is BodyAction)
                    return ToDelegate((BodyAction)body);

                return (BodyDelegate)body;
            }
            set { this[RequestBodyKey] = value; }
        }
        static BodyDelegate ToDelegate(BodyAction body)
        {
            return (write, flush, end, cancellationToken) =>
            {
                var cancel = body(
                    (data, continuation) => write(data),
                    end,
                    () => end(null));
                cancellationToken.Register(cancel);
            };
        }
        /// <summary>
        /// "owin.QueryString" A string containing the query string component of the HTTP request URI (e.g., "foo=bar&baz=quux"). The value may be an empty string.
        /// </summary>
        public string QueryString
        {
            get { return Get<string>(RequestQueryStringKey); }
            set { this[RequestQueryStringKey] = value; }
        }

        /// <summary>
        /// "host.CallDisposed" A CancellationToken that is triggered after the request/response has finished executing or has failed.
        /// </summary>
        public CancellationToken CallDisposed
        {
            get { return Get<CancellationToken>("host.CallDisposed"); }
            set { this["host.CallDisposed"] = value; }
        }


        /// <summary>
        /// "host.TraceOutput" A TextWriter that directs trace or logger output to an appropriate place for the host
        /// </summary>
        public TextWriter TraceOutput
        {
            get { return Get<TextWriter>("host.TraceOutput"); }
            set { this["host.TraceOutput"] = value; }
        }


        #region Implementation of IEnumerable

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Env.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<KeyValuePair<string,object>>

        public void Add(KeyValuePair<string, object> item)
        {
            Env.Add(item);
        }

        public void Clear()
        {
            Env.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return Env.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            Env.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return Env.Remove(item);
        }

        public int Count
        {
            get { return Env.Count; }
        }

        public bool IsReadOnly
        {
            get { return Env.IsReadOnly; }
        }

        #endregion

        #region Implementation of IDictionary<string,object>

        public bool ContainsKey(string key)
        {
            return Env.ContainsKey(key);
        }

        public void Add(string key, object value)
        {
            Env.Add(key, value);
        }

        public bool Remove(string key)
        {
            return Env.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return Env.TryGetValue(key, out value);
        }

        public object this[string key]
        {
            get { return Env[key]; }
            set { Env[key] = value; }
        }

        public ICollection<string> Keys
        {
            get { return Env.Keys; }
        }

        public ICollection<object> Values
        {
            get { return Env.Values; }
        }

        #endregion
    }
}