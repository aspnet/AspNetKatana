using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gate.Utils;
//using Owin;

namespace Gate
{
    // A helper object for creating, modifying, or consuming request data in an Environment dictionary.
    public struct Request
    {
        IDictionary<string, object> environment;
        static readonly char[] CommaSemicolon = new[] { ',', ';' };

        // Primarily for unit tests
        public static Request Create()
        {
            Request request = new Request(new Dictionary<string, object>());
            request.Set(OwinConstants.RequestHeaders, Gate.Headers.New());
            request.Set(OwinConstants.ResponseHeaders, Gate.Headers.New());
            return request;
        }

        public Request(IDictionary<string, object> environment)
        {
            this.environment = environment;
        }

        public IDictionary<string, object> Environment
        {
            get { return environment; }
            set { environment = value; }
        }

        // Retrieves the value if present, or returns the default (null) otherwise.
        public T Get<T>(string key)
        {
            object value;
            return Environment.TryGetValue(key, out value) ? (T)value : default(T);
        }

        // Sets the value if non-null, or removes it otherwise
        public Request Set<T>(string key, T value)
        {
            if (object.Equals(value, default(T)))
            {
                Environment.Remove(key);
            }
            else
            {
                Environment[key] = value;
            }
            return this;
        }

        public IDictionary<string, string[]> Headers
        {
            get { return Get<IDictionary<string, string[]>>(OwinConstants.RequestHeaders); }
            set { Set<IDictionary<string, string[]>>(OwinConstants.RequestHeaders, value); }
        }

        public Stream Body
        {
            get { return Get<Stream>(OwinConstants.RequestBody); }
            set { Set<Stream>(OwinConstants.RequestBody, value); }
        }

        public CancellationToken CancellationToken
        {
            get { return Get<CancellationToken>(OwinConstants.CallCancelled); }
            set { Set<CancellationToken>(OwinConstants.CallCancelled, value); }
        }

        /// <summary>
        /// "owin.Version" The string "1.0" indicating OWIN version 1.0. 
        /// </summary>
        public string Version
        {
            get { return Get<string>(OwinConstants.Version); }
            set { Set<string>(OwinConstants.Version, value); }
        }

        /// <summary>
        /// "owin.RequestProtocol" A string containing the protocol name and version (e.g. "HTTP/1.0" or "HTTP/1.1"). 
        /// </summary>
        public string Protocol
        {
            get { return Get<string>(OwinConstants.RequestProtocol); }
            set { Set<string>(OwinConstants.RequestProtocol, value); }
        }

        /// <summary>
        /// "owin.RequestMethod" A string containing the HTTP request method of the request (e.g., "GET", "POST"). 
        /// </summary>
        public string Method
        {
            get { return Get<string>(OwinConstants.RequestMethod); }
            set { Set<string>(OwinConstants.RequestMethod, value); }
        }

        /// <summary>
        /// "owin.RequestScheme" A string containing the URI scheme used for the request (e.g., "http", "https").  
        /// </summary>
        public string Scheme
        {
            get { return Get<string>(OwinConstants.RequestScheme); }
            set { Set<string>(OwinConstants.RequestScheme, value); }
        }

        /// <summary>
        /// "owin.RequestPathBase" A string containing the portion of the request path corresponding to the "root" of the application delegate. The value may be an empty string.  
        /// </summary>
        public string PathBase
        {
            get { return Get<string>(OwinConstants.RequestPathBase); }
            set { Set<string>(OwinConstants.RequestPathBase, value); }
        }

        /// <summary>
        /// "owin.RequestPath" A string containing the request path. The path must be relative to the "root" of the application delegate. 
        /// </summary>
        public string Path
        {
            get { return Get<string>(OwinConstants.RequestPath); }
            set { Set<string>(OwinConstants.RequestPath, value); }
        }

        /// <summary>
        /// "owin.QueryString" A string containing the query string component of the HTTP request URI (e.g., "foo=bar&baz=quux"). The value may be an empty string.
        /// </summary>
        public string QueryString
        {
            get { return Get<string>(OwinConstants.RequestQueryString); }
            set { Set<string>(OwinConstants.RequestQueryString, value); }
        }


        /// <summary>
        /// "host.TraceOutput" A TextWriter that directs trace or logger output to an appropriate place for the host
        /// </summary>
        public TextWriter TraceOutput
        {
            get { return Get<TextWriter>(OwinConstants.TraceOutput); }
            set { Set<TextWriter>(OwinConstants.TraceOutput, value); }
        }

        public IDictionary<string, string> Query
        {
            get
            {
                var text = QueryString;
                if (Get<string>("Gate.Request.Query#text") != text ||
                    Get<IDictionary<string, string>>("Gate.Request.Query") == null)
                {
                    Set<string>("Gate.Request.Query#text", text);
                    Set<IDictionary<string, string>>("Gate.Request.Query", ParamDictionary.Parse(text));
                }
                return Get<IDictionary<string, string>>("Gate.Request.Query");
            }
        }

        public IDictionary<string, string> Cookies
        {
            get
            {
                var cookies = Get<IDictionary<string, string>>("Gate.Request.Cookies#dictionary");
                if (cookies == null)
                {
                    cookies = new Dictionary<string, string>(StringComparer.Ordinal);
                    Set("Gate.Request.Cookies#dictionary", cookies);
                }

                var text = Headers.GetHeader("Cookie");
                if (Get<string>("Gate.Request.Cookies#text") != text)
                {
                    cookies.Clear();
                    foreach (var kv in ParamDictionary.ParseToEnumerable(text, CommaSemicolon))
                    {
                        if (!cookies.ContainsKey(kv.Key))
                            cookies.Add(kv);
                    }
                    Set("Gate.Request.Cookies#text", text);
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

        public Task CopyToStreamAsync(Stream stream)
        {
            if (Body == null)
            {
                return TaskHelpers.Completed();
            }
            if (Body.CanSeek)
            {
                Body.Seek(0, SeekOrigin.Begin);
            }
            return Body.CopyToAsync(stream);
        }

        public void CopyToStream(Stream stream)
        {
            if (Body == null)
            {
                return;
            }
            if (Body.CanSeek)
            {
                Body.Seek(0, SeekOrigin.Begin);
            }
            Body.CopyTo(stream);
        }


        public Task<string> ReadTextAsync()
        {
            var text = Get<string>("Gate.Request.Text");

            var thisInput = Body;
            var lastInput = Get<object>("Gate.Request.Text#input");

            if (text != null && ReferenceEquals(thisInput, lastInput))
            {
                return TaskHelpers.FromResult(text);
            }

            var buffer = new MemoryStream();

            Request thisRequest = this;

            //TODO: determine encoding from request content type
            return CopyToStreamAsync(buffer)
                .Then(() =>
                {
                    buffer.Seek(0, SeekOrigin.Begin);
                    text = new StreamReader(buffer).ReadToEnd();
                    thisRequest.Environment["Gate.Request.Text#input"] = thisInput;
                    thisRequest.Environment["Gate.Request.Text"] = text;
                    return text;
                });
        }

        public string ReadText()
        {
            var text = Get<string>("Gate.Request.Text");

            var thisInput = Body;
            var lastInput = Get<object>("Gate.Request.Text#input");

            if (text != null && ReferenceEquals(thisInput, lastInput))
            {
                return text;
            }

            if (thisInput != null)
            {
                if (thisInput.CanSeek)
                {
                    thisInput.Seek(0, SeekOrigin.Begin);
                }
                text = new StreamReader(thisInput).ReadToEnd();
            }

            Set("Gate.Request.Text#input", thisInput);
            Set("Gate.Request.Text", text);
            return text;
        }

        public Task<IDictionary<string, string>> ReadFormAsync()
        {
            if (!HasFormData && !HasParseableData)
            {
                return TaskHelpers.FromResult(ParamDictionary.Parse(""));
            }

            var form = Get<IDictionary<string, string>>("Gate.Request.Form");
            var thisInput = Body;
            var lastInput = Get<object>("Gate.Request.Form#input");
            if (form != null && ReferenceEquals(thisInput, lastInput))
            {
                return TaskHelpers.FromResult(form);
            }

            Request thisRequest = this;

            return ReadTextAsync().Then(text =>
            {
                form = ParamDictionary.Parse(text);
                thisRequest.Set("Gate.Request.Form#input", thisInput);
                thisRequest.Set("Gate.Request.Form", form);
                return form;
            });
        }

        public IDictionary<string, string> ReadForm()
        {
            if (!HasFormData && !HasParseableData)
            {
                return ParamDictionary.Parse("");
            }

            var form = Get<IDictionary<string, string>>("Gate.Request.Form");
            var thisInput = Body;
            var lastInput = Get<object>("Gate.Request.Form#input");
            if (form != null && ReferenceEquals(thisInput, lastInput))
            {
                return form;
            }

            var text = ReadText();
            form = ParamDictionary.Parse(text);
            Set("Gate.Request.Form#input", thisInput);
            Set("Gate.Request.Form", form);
            return form;
        }



        bool TryParseHostHeader(out IPAddress address, out string host, out int port)
        {
            address = null;
            host = null;
            port = 0;

            var hostHeader = Headers.GetHeader("Host");
            if (string.IsNullOrWhiteSpace(hostHeader))
            {
                return false;
            }

            if (hostHeader.StartsWith("[", StringComparison.Ordinal))
            {
                var portIndex = hostHeader.LastIndexOf("]:", StringComparison.Ordinal);
                if (portIndex != -1 && int.TryParse(hostHeader.Substring(portIndex + 2), out port))
                {
                    if (IPAddress.TryParse(hostHeader.Substring(1, portIndex - 1), out address))
                    {
                        host = null;
                        return true;
                    }
                    host = hostHeader.Substring(0, portIndex + 1);
                    return true;
                }
                if (hostHeader.EndsWith("]", StringComparison.Ordinal))
                {
                    if (IPAddress.TryParse(hostHeader.Substring(1, hostHeader.Length - 2), out address))
                    {
                        host = null;
                        port = 0;
                        return true;
                    }
                }
            }
            else
            {
                if (IPAddress.TryParse(hostHeader, out address))
                {
                    host = null;
                    port = 0;
                    return true;
                }

                var portIndex = hostHeader.LastIndexOf(':');
                if (portIndex != -1 && int.TryParse(hostHeader.Substring(portIndex + 1), out port))
                {
                    host = hostHeader.Substring(0, portIndex);
                    return true;
                }
            }

            host = hostHeader;
            return true;
        }

        void AssignHostHeader(
           string host,
           int port)
        {
            IPAddress address;
            if (IPAddress.TryParse(host, out address))
            {
                Headers.SetHeader("Host", new IPEndPoint(address, port).ToString());
            }
            else
            {
                Headers.SetHeader("Host", host + ":" + port);
            }
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

                var localIpAddressString = Get<string>(OwinConstants.LocalIpAddress);
                IPAddress localIpAddress;
                if (!IPAddress.TryParse(localIpAddressString, out localIpAddress))
                {
                    localIpAddress = IPAddress.Loopback;
                }

                var localPortString = Get<string>(OwinConstants.LocalPort);
                int localPort;
                if (!int.TryParse(localPortString, out localPort))
                {
                    localPort = string.Equals(Scheme, "https", StringComparison.OrdinalIgnoreCase) ? 443 : 80;
                }

                return new IPEndPoint(localIpAddress, localPort).ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Headers.Remove("Host");
                }
                else
                {
                    Headers.SetHeader("Host", value);
                }
            }
        }

        public string Host
        {
            get
            {
                IPAddress address;
                string host;
                int port;
                if (TryParseHostHeader(out address, out host, out port))
                {
                    return host ?? address.ToString();
                }
                return Get<string>(OwinConstants.LocalIpAddress) ?? IPAddress.Loopback.ToString();
            }
            set
            {
                var host = value;
                var port = Port;

                AssignHostHeader(host, port);
            }
        }

        public int Port
        {
            get
            {
                IPAddress address;
                string host;
                int port;
                if (TryParseHostHeader(out address, out host, out port) && port != 0)
                {
                    return port;
                }
                var portString = Get<string>(OwinConstants.LocalPort);
                if (int.TryParse(portString, out port) && port != 0)
                {
                    return port;
                }
                return string.Equals(Scheme, "https", StringComparison.OrdinalIgnoreCase) ? 443 : 80;
            }
            set
            {
                var host = Host;
                var port = value;

                if (port == 0)
                {
                    HostWithPort = host;

                }
                else
                {
                    AssignHostHeader(host, port);
                }
            }
        }
    }
}