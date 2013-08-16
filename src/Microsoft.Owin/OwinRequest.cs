// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Threading;
using Microsoft.Owin.Infrastructure;

namespace Microsoft.Owin
{
    /// <summary>
    /// This wraps OWIN environment dictionary and provides strongly typed accessors.
    /// </summary>
    public partial class OwinRequest : IOwinRequest
    {
        /// <summary>
        /// Create a new context with only request and response header collections.
        /// </summary>
        public OwinRequest()
        {
            IDictionary<string, object> environment = new Dictionary<string, object>(StringComparer.Ordinal);
            environment[OwinConstants.RequestHeaders] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            environment[OwinConstants.ResponseHeaders] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            Environment = environment;
        }

        /// <summary>
        /// Create a new environment wrapper exposing request properties.
        /// </summary>
        /// <param name="environment"></param>
        public OwinRequest(IDictionary<string, object> environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            Environment = environment;
        }

        /// <summary>
        /// The wrapped OWIN environment.
        /// </summary>
        public virtual IDictionary<string, object> Environment { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual IOwinContext Context
        {
            get { return new OwinContext(Environment); }
        }

        /// <summary>
        /// The HTTP method/verb, e.g. GET, POST, etc..
        /// </summary>
        public virtual string Method
        {
            get { return Get<string>(OwinConstants.RequestMethod); }
            set { Set(OwinConstants.RequestMethod, value); }
        }

        /// <summary>
        /// The HTTP request scheme (e.g. http or https) from owin.RequestScheme.
        /// </summary>
        public virtual string Scheme
        {
            get { return Get<string>(OwinConstants.RequestScheme); }
            set { Set(OwinConstants.RequestScheme, value); }
        }

        /// <summary>
        /// Returns true if the owin.RequestScheme is https.
        /// </summary>
        public virtual bool IsSecure
        {
            get { return string.Equals(Scheme, Constants.Https, StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>
        /// The request host, taken from the Host request header in owin.RequestHeaders.
        /// May include the port.
        /// </summary>
        public virtual HostString Host
        {
            get { return new HostString(OwinHelpers.GetHost(this)); }
            set { OwinHelpers.SetHeader(RawHeaders, Constants.Headers.Host, value.Value); }
        }

        /// <summary>
        /// See owin.RequestPathBase.
        /// </summary>
        public virtual PathString PathBase
        {
            get { return new PathString(Get<string>(OwinConstants.RequestPathBase)); }
            set { Set(OwinConstants.RequestPathBase, value.Value); }
        }

        /// <summary>
        /// The request path from owin.RequestPath.
        /// </summary>
        public virtual PathString Path
        {
            get { return new PathString(Get<string>(OwinConstants.RequestPath)); }
            set { Set(OwinConstants.RequestPath, value.Value); }
        }

        /// <summary>
        /// The query string from owin.RequestQueryString.
        /// </summary>
        public virtual QueryString QueryString
        {
            get { return new QueryString(Get<string>(OwinConstants.RequestQueryString)); }
            set { Set(OwinConstants.RequestQueryString, value.Value); }
        }

        /// <summary>
        /// owin.RequestQueryString parsed into a collection
        /// </summary>
        public virtual IReadableStringCollection Query
        {
            get { return new ReadableStringCollection(OwinHelpers.GetQuery(this)); }
        }

        /// <summary>
        /// A Uri with the combine parts of owin.RequestScheme, the Host header, owin.RequestPathBase, owin.RequestPath, and owin.RequestQueryString.
        /// </summary>
        public virtual Uri Uri
        {
            get { return new Uri(Scheme + Uri.SchemeDelimiter + Host + PathBase + Path + QueryString); }
        }

        /// <summary>
        /// owin.RequestProtocol
        /// </summary>
        public virtual string Protocol
        {
            get { return Get<string>(OwinConstants.RequestProtocol); }
            set { Set(OwinConstants.RequestProtocol, value); }
        }

        /// <summary>
        /// owin.RequestHeaders in a wrapper
        /// </summary>
        public virtual IHeaderDictionary Headers
        {
            get { return new HeaderDictionary(RawHeaders); }
        }

        private IDictionary<string, string[]> RawHeaders
        {
            get { return Get<IDictionary<string, string[]>>(OwinConstants.RequestHeaders); }
        }

        /// <summary>
        /// The Cookie header parsed into a collection
        /// </summary>
        public RequestCookieCollection Cookies
        {
            get { return new RequestCookieCollection(OwinHelpers.GetCookies(this)); }
        }

        /// <summary>
        /// The Content-Type header
        /// </summary>
        public virtual string ContentType
        {
            get { return OwinHelpers.GetHeader(RawHeaders, Constants.Headers.ContentType); }
            set { OwinHelpers.SetHeader(RawHeaders, Constants.Headers.ContentType, value); }
        }

        /// <summary>
        /// The Cache-Control header
        /// </summary>
        public virtual string CacheControl
        {
            get { return OwinHelpers.GetHeader(RawHeaders, Constants.Headers.CacheControl); }
            set { OwinHelpers.SetHeader(RawHeaders, Constants.Headers.CacheControl, value); }
        }

        /// <summary>
        /// The Media-Type header
        /// </summary>
        public virtual string MediaType
        {
            get { return OwinHelpers.GetHeader(RawHeaders, Constants.Headers.MediaType); }
            set { OwinHelpers.SetHeader(RawHeaders, Constants.Headers.MediaType, value); }
        }

        /// <summary>
        /// The Accept header
        /// </summary>
        public virtual string Accept
        {
            get { return OwinHelpers.GetHeader(RawHeaders, Constants.Headers.Accept); }
            set { OwinHelpers.SetHeader(RawHeaders, Constants.Headers.Accept, value); }
        }

        /// <summary>
        /// The owin.RequestBody Stream.
        /// </summary>
        public virtual Stream Body
        {
            get { return Get<Stream>(OwinConstants.RequestBody); }
            set { Set(OwinConstants.RequestBody, value); }
        }

        /// <summary>
        /// owin.CallCancelled
        /// </summary>
        public virtual CancellationToken CallCancelled
        {
            get { return Get<CancellationToken>(OwinConstants.CallCancelled); }
            set { Set(OwinConstants.CallCancelled, value); }
        }

        /// <summary>
        /// server.LocalIpAddress
        /// </summary>
        public virtual string LocalIpAddress
        {
            get { return Get<string>(OwinConstants.CommonKeys.LocalIpAddress); }
            set { Set(OwinConstants.CommonKeys.LocalIpAddress, value); }
        }

        /// <summary>
        /// server.LocalPort
        /// </summary>
        public virtual int? LocalPort
        {
            get
            {
                int value;
                if (int.TryParse(LocalPortString, out value))
                {
                    return value;
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    LocalPortString = value.Value.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    Environment.Remove(OwinConstants.CommonKeys.LocalPort);
                }
            }
        }

        private string LocalPortString
        {
            get { return Get<string>(OwinConstants.CommonKeys.LocalPort); }
            set { Set(OwinConstants.CommonKeys.LocalPort, value); }
        }

        /// <summary>
        /// server.RemoteIpAddress
        /// </summary>
        public virtual string RemoteIpAddress
        {
            get { return Get<string>(OwinConstants.CommonKeys.RemoteIpAddress); }
            set { Set(OwinConstants.CommonKeys.RemoteIpAddress, value); }
        }

        /// <summary>
        /// server.RemotePort
        /// </summary>
        public virtual int? RemotePort
        {
            get
            {
                int value;
                if (int.TryParse(RemotePortString, out value))
                {
                    return value;
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    RemotePortString = value.Value.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    Environment.Remove(OwinConstants.CommonKeys.RemotePort);
                }
            }
        }

        private string RemotePortString
        {
            get { return Get<string>(OwinConstants.CommonKeys.RemotePort); }
            set { Set(OwinConstants.CommonKeys.RemotePort, value); }
        }

        /// <summary>
        /// server.User.
        /// </summary>
        public virtual IPrincipal User
        {
            get { return Get<IPrincipal>(OwinConstants.Security.User); }
            set { Set(OwinConstants.Security.User, value); }
        }

        /// <summary>
        /// Gets a value from the OWIN environment, or returns default(T) if not present.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual T Get<T>(string key)
        {
            object value;
            return Environment.TryGetValue(key, out value) ? (T)value : default(T);
        }

        /// <summary>
        /// Sets the given key and value in the OWIN environment.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public virtual IOwinRequest Set<T>(string key, T value)
        {
            Environment[key] = value;
            return this;
        }
    }
}
