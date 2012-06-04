using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Gate.Utils;

namespace Gate
{
    using BodyAction = Func<Func<ArraySegment<byte>, Action, bool>, Action<Exception>, Action, Action>;
    
    internal class Request : Environment
    {
        static readonly char[] CommaSemicolon = new[] {',', ';'};

        public Request(IDictionary<string, object> env) : base(env)
        {
        }

        public IDictionary<string, string> Query
        {
            get
            {
                var text = QueryString;
                if (Get<string>("Gate.Helpers.Request.Query:text") != text ||
                    Get<IDictionary<string, string>>("Gate.Helpers.Request.Query") == null)
                {
                    this["Gate.Helpers.Request.Query:text"] = text;
                    this["Gate.Helpers.Request.Query"] = ParamDictionary.Parse(text);
                }
                return Get<IDictionary<string, string>>("Gate.Helpers.Request.Query");
            }
        }

        static readonly char[] CookieParamSeparators = new[] { ';', ',' };
        public IDictionary<string, string> Cookies
        {
            get
            {
                var cookies = Get<IDictionary<string, string>>("Gate.Request.Cookies#dictionary");
                if (cookies == null)
                {
                    cookies = new Dictionary<string, string>(StringComparer.Ordinal);
                    Env["Gate.Request.Cookies#dictionary"] = cookies;
                }

                var text = Headers.GetHeader("Cookie");
                if (Get<string>("Gate.Request.Cookies#text") != text)
                {
                    cookies.Clear();
                    foreach (var kv in ParamDictionary.ParseToEnumerable(text, CookieParamSeparators))
                    {
                        if (!cookies.ContainsKey(kv.Key))
                            cookies.Add(kv);
                    }
                    Env["Gate.Request.Cookies#text"] = text;
                }
                return cookies;
            }
        }

        public bool HasFormData
        {
            get
            {
                var mediaType = MediaType;
                return (Method == "POST" && string.IsNullOrEmpty(mediaType))
                    || mediaType == "application/x-www-form-urlencoded"
                        || mediaType == "multipart/form-data";
            }
        }

        public bool HasParseableData
        {
            get
            {
                var mediaType = MediaType;
                return mediaType == "application/x-www-form-urlencoded"
                    || mediaType == "multipart/form-data";
            }
        }


        public string ContentType
        {
            get
            {
                return Headers.GetHeader("Content-Type");
            }
        }

        public string MediaType
        {
            get
            {
                var contentType = ContentType;
                if (contentType == null)
                    return null;
                var delimiterPos = contentType.IndexOfAny(CommaSemicolon);
                return delimiterPos < 0 ? contentType : contentType.Substring(0, delimiterPos);
            }
        }


        public IDictionary<string, string> Post
        {
            get
            {
                if (HasFormData || HasParseableData)
                {
                    var input = BodyAction;
                    if (input == null)
                    {
                        throw new InvalidOperationException("Missing input");
                    }

                    if (!ReferenceEquals(Get<object>("Gate.Helpers.Request.Post:input"), input) ||
                        Get<IDictionary<string, string>>("Gate.Helpers.Request.Post") == null)
                    {
                        var text = ToText(input, Encoding.UTF8);
                        this["Gate.Helpers.Request.Post:input"] = input;
                        this["Gate.Helpers.Request.Post:text"] = text;
                        this["Gate.Helpers.Request.Post"] = ParamDictionary.Parse(text);
                    }
                    return Get<IDictionary<string, string>>("Gate.Helpers.Request.Post");
                }

                return ParamDictionary.Parse("");
            }
        }


        static string ToText(BodyAction body, Encoding encoding)
        {
            var sb = new StringBuilder();
            var wait = new ManualResetEvent(false);
            Exception exception = null;
            body.Invoke(
                (data, _) =>
                {
                    sb.Append(encoding.GetString(data.Array, data.Offset, data.Count));
                    return false;
                },
                ex =>
                {
                    exception = ex;
                    wait.Set();
                },
                () => wait.Set());

            wait.WaitOne();
            if (exception != null)
                throw new AggregateException(exception);
            return sb.ToString();
        }


        public string HostWithPort
        {
            get
            {
                var hostHeader = Headers.GetHeader("Host");
                if (!string.IsNullOrWhiteSpace(hostHeader))
                {
                    return hostHeader;
                }

                var serverName = Get<string>("server.SERVER_NAME");
                if (string.IsNullOrWhiteSpace(serverName))
                    serverName = Get<string>("server.SERVER_ADDRESS");
                var serverPort = Get<string>("server.SERVER_PORT");

                return serverName + ":" + serverPort;
            }
        }

        public string Host
        {
            get
            {
                var hostHeader = Headers.GetHeader("Host");
                if (!string.IsNullOrWhiteSpace(hostHeader))
                {
                    var delimiter = hostHeader.IndexOf(':');
                    return delimiter < 0 ? hostHeader : hostHeader.Substring(0, delimiter);
                }
                var serverName = Get<string>("server.SERVER_NAME");
                if (string.IsNullOrWhiteSpace(serverName))
                    serverName = Get<string>("server.SERVER_ADDRESS");
                return serverName;
            }
        }
    }
}