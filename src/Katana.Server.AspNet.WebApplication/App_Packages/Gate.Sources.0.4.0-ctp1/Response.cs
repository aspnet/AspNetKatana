using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Owin;
using Gate.Utils;

namespace Gate
{
    internal class Response
    {
        ResultDelegate _result;

        int _autostart;
        readonly Object _onStartSync = new object();
        Action _onStart = () => { };

        Func<ArraySegment<byte>, Action, bool> _responseWrite;
        Action<Exception> _responseEnd;
        CancellationToken _responseCancellationToken = CancellationToken.None;

        public Response(ResultDelegate result)
            : this(result, "200 OK")
        {
        }

        public Response(ResultDelegate result, string status)
            : this(result, status, new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase))
        {
        }

        public Response(ResultDelegate result, string status, IDictionary<string, string[]> headers)
        {
            _result = result;

            _responseWrite = EarlyResponseWrite;
            _responseEnd = EarlyResponseEnd;

            Status = status;
            Headers = headers;
            Encoding = Encoding.UTF8;
        }

        public string Status { get; set; }
        public IDictionary<string, string[]> Headers { get; set; }
        public Encoding Encoding { get; set; }
        public bool Buffer { get; set; }

        public string GetHeader(string name)
        {
            var values = GetHeaders(name);
            if (values == null)
            {
                return null;
            }

            switch (values.Length)
            {
                case 0:
                    return string.Empty;
                case 1:
                    return values[0];
                default:
                    return string.Join(",", values);
            }
        }

        public string[] GetHeaders(string name)
        {
            string[] existingValues;
            return Headers.TryGetValue(name, out existingValues) ? existingValues : null;
        }

        public Response SetHeader(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                Headers.Remove(value);
            else
                Headers[name] = new[] { value };
            return this;
        }

        public Response SetCookie(string key, string value)
        {
            Headers.AddHeader("Set-Cookie", Uri.EscapeDataString(key) + "=" + Uri.EscapeDataString(value) + "; path=/");
            return this;
        }

        public Response SetCookie(string key, Cookie cookie)
        {
            var domainHasValue = !string.IsNullOrEmpty(cookie.Domain);
            var pathHasValue = !string.IsNullOrEmpty(cookie.Path);
            var expiresHasValue = cookie.Expires.HasValue;

            var setCookieValue = string.Concat(
                Uri.EscapeDataString(key),
                "=",
                Uri.EscapeDataString(cookie.Value ?? ""), //TODO: concat complex value type with '&'?
                !domainHasValue ? null : "; domain=",
                !domainHasValue ? null : cookie.Domain,
                !pathHasValue ? null : "; path=",
                !pathHasValue ? null : cookie.Path,
                !expiresHasValue ? null : "; expires=",
                !expiresHasValue ? null : cookie.Expires.Value.ToString("ddd, dd-MMM-yyyy HH:mm:ss ") + "GMT",
                !cookie.Secure ? null : "; secure",
                !cookie.HttpOnly ? null : "; HttpOnly"
                );
            Headers.AddHeader("Set-Cookie", setCookieValue);
            return this;
        }

        public Response DeleteCookie(string key)
        {
            Func<string, bool> predicate = value => value.StartsWith(key + "=", StringComparison.InvariantCultureIgnoreCase);

            var deleteCookies = new[] { Uri.EscapeDataString(key) + "=; expires=Thu, 01-Jan-1970 00:00:00 GMT" };
            var existingValues = Headers.GetHeaders("Set-Cookie");
            if (existingValues == null)
            {
                Headers["Set-Cookie"] = deleteCookies;
                return this;
            }

            Headers["Set-Cookie"] = existingValues.Where(value => !predicate(value)).Concat(deleteCookies).ToArray();
            return this;
        }

        public Response DeleteCookie(string key, Cookie cookie)
        {
            var domainHasValue = !string.IsNullOrEmpty(cookie.Domain);
            var pathHasValue = !string.IsNullOrEmpty(cookie.Path);

            Func<string, bool> rejectPredicate;
            if (domainHasValue)
            {
                rejectPredicate = value =>
                    value.StartsWith(key + "=", StringComparison.InvariantCultureIgnoreCase) &&
                        value.IndexOf("domain=" + cookie.Domain, StringComparison.InvariantCultureIgnoreCase) != -1;
            }
            else if (pathHasValue)
            {
                rejectPredicate = value =>
                    value.StartsWith(key + "=", StringComparison.InvariantCultureIgnoreCase) &&
                        value.IndexOf("path=" + cookie.Path, StringComparison.InvariantCultureIgnoreCase) != -1;
            }
            else
            {
                rejectPredicate = value => value.StartsWith(key + "=", StringComparison.InvariantCultureIgnoreCase);
            }
            var existingValues = Headers.GetHeaders("Set-Cookie");
            if (existingValues != null)
            {
                Headers["Set-Cookie"] = existingValues.Where(value => !rejectPredicate(value)).ToArray();
            }

            return SetCookie(key, new Cookie
            {
                Path = cookie.Path,
                Domain = cookie.Domain,
                Expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            });
        }


        internal class Cookie
        {
            public Cookie()
            {
                Path = "/";
            }
            public Cookie(string value)
            {
                Path = "/";
                Value = value;
            }
            public string Value { get; set; }
            public string Domain { get; set; }
            public string Path { get; set; }
            public DateTime? Expires { get; set; }
            public bool Secure { get; set; }
            public bool HttpOnly { get; set; }
        }

        public string ContentType
        {
            get { return GetHeader("Content-Type"); }
            set { SetHeader("Content-Type", value); }
        }


        public Response Start()
        {
            _autostart = 1;
            Interlocked.Exchange(ref _result, ResultCalledAlready).Invoke(Status, Headers, ResponseBody);
            return this;
        }

        public Response Start(string status)
        {
            if (!string.IsNullOrWhiteSpace(status))
                Status = status;

            return Start();
        }

        public Response Start(string status, IEnumerable<KeyValuePair<string, string[]>> headers)
        {
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    Headers[header.Key] = header.Value;
                }
            }
            return Start(status);
        }

        public void Start(string status, IEnumerable<KeyValuePair<string, string>> headers)
        {
            var actualHeaders = headers.Select(kv => new KeyValuePair<string, string[]>(kv.Key, new[] { kv.Value }));
            Start(status, actualHeaders);
        }

        public void Start(Action continuation)
        {
            OnStart(continuation);
            Start();
        }

        public void Start(string status, Action continuation)
        {
            OnStart(continuation);
            Start(status);
        }

        public Response Write(string text)
        {
            // this could be more efficient if it spooled the immutable strings instead...
            var data = Encoding.GetBytes(text);
            return Write(new ArraySegment<byte>(data));
        }

        public Response Write(string format, params object[] args)
        {
            return Write(string.Format(format, args));
        }

        public Response Write(ArraySegment<byte> data)
        {
            _responseWrite(data, null);
            return this;
        }

        public void End()
        {
            OnEnd(null);
        }

        public void End(string text)
        {
            Write(text);
            OnEnd(null);
        }

        public void End(ArraySegment<byte> data)
        {
            Write(data);
            OnEnd(null);
        }

        public void Error(Exception error)
        {
            OnEnd(error);
        }

        void ResponseBody(
            Func<ArraySegment<byte>, Action, bool> write,
            Action<Exception> end,
            CancellationToken cancellationToken)
        {
            _responseWrite = write;
            _responseEnd = end;
            _responseCancellationToken = cancellationToken;
            lock (_onStartSync)
            {
                Interlocked.Exchange(ref _onStart, null).Invoke();
            }
        }


        static readonly ResultDelegate ResultCalledAlready =
            (_, __, ___) =>
            {
                throw new InvalidOperationException("Start must only be called once on a Response and it must be called before Write or End");
            };

        void Autostart()
        {
            if (Interlocked.Increment(ref _autostart) == 1)
            {
                Start();
            }
        }


        void OnStart(Action notify)
        {
            lock (_onStartSync)
            {
                if (_onStart != null)
                {
                    var prior = _onStart;
                    _onStart = () =>
                    {
                        prior.Invoke();
                        CallNotify(notify);
                    };
                    return;
                }
            }
            CallNotify(notify);
        }

        void OnEnd(Exception error)
        {
            Interlocked.Exchange(ref _responseEnd, _ => { }).Invoke(error);
        }

        void CallNotify(Action notify)
        {
            try
            {
                notify.Invoke();
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        bool EarlyResponseWrite(ArraySegment<byte> data, Action callback)
        {
            var copy = data;
            if (copy.Count != 0)
            {
                copy = new ArraySegment<byte>(new byte[data.Count], 0, data.Count);
                Array.Copy(data.Array, data.Offset, copy.Array, 0, data.Count);
            }
            OnStart(
                () =>
                {
                    var willCallback = _responseWrite(copy, callback);
                    if (callback != null && willCallback == false)
                    {
                        callback.Invoke();
                    }
                });
            if (!Buffer || data.Array == null)
            {
                Autostart();
            }
            return true;
        }


        void EarlyResponseEnd(Exception ex)
        {
            OnStart(() => OnEnd(ex));
            Autostart();
        }


    }
}