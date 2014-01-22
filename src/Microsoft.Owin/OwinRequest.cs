// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Infrastructure;

namespace Microsoft.Owin
{
    /// <summary>
    /// This wraps OWIN environment dictionary and provides strongly typed accessors.
    /// </summary>
    public class OwinRequest : IOwinRequest
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
        /// <param name="environment">OWIN environment dictionary which stores state information about the request, response and relevant server state.</param>
        public OwinRequest(IDictionary<string, object> environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            Environment = environment;
        }

        /// <summary>
        /// Gets the OWIN environment.
        /// </summary>
        /// <returns>The OWIN environment.</returns>
        public virtual IDictionary<string, object> Environment { get; private set; }

        /// <summary>
        /// Gets the request context.
        /// </summary>
        /// <returns>The request context.</returns>
        public virtual IOwinContext Context
        {
            get { return new OwinContext(Environment); }
        }

        /// <summary>
        /// Gets or set the HTTP method.
        /// </summary>
        /// <returns>The HTTP method.</returns>
        public virtual string Method
        {
            get { return Get<string>(OwinConstants.RequestMethod); }
            set { Set(OwinConstants.RequestMethod, value); }
        }

        /// <summary>
        /// Gets or set the HTTP request scheme from owin.RequestScheme.
        /// </summary>
        /// <returns>The HTTP request scheme from owin.RequestScheme.</returns>
        public virtual string Scheme
        {
            get { return Get<string>(OwinConstants.RequestScheme); }
            set { Set(OwinConstants.RequestScheme, value); }
        }

        /// <summary>
        /// Returns true if the owin.RequestScheme is https.
        /// </summary>
        /// <returns>true if this request is using https; otherwise, false.</returns>
        public virtual bool IsSecure
        {
            get { return string.Equals(Scheme, Constants.Https, StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>
        /// Gets or set the Host header. May include the port.
        /// </summary>
        /// <return>The Host header.</return>
        public virtual HostString Host
        {
            get { return new HostString(OwinHelpers.GetHost(this)); }
            set { OwinHelpers.SetHeader(RawHeaders, Constants.Headers.Host, value.Value); }
        }

        /// <summary>
        /// Gets or set the owin.RequestPathBase.
        /// </summary>
        /// <returns>The owin.RequestPathBase.</returns>
        public virtual PathString PathBase
        {
            get { return new PathString(Get<string>(OwinConstants.RequestPathBase)); }
            set { Set(OwinConstants.RequestPathBase, value.Value); }
        }

        /// <summary>
        /// Gets or set the request path from owin.RequestPath.
        /// </summary>
        /// <returns>The request path from owin.RequestPath.</returns>
        public virtual PathString Path
        {
            get { return new PathString(Get<string>(OwinConstants.RequestPath)); }
            set { Set(OwinConstants.RequestPath, value.Value); }
        }

        /// <summary>
        /// Gets or set the query string from owin.RequestQueryString.
        /// </summary>
        /// <returns>The query string from owin.RequestQueryString.</returns>
        public virtual QueryString QueryString
        {
            get { return new QueryString(Get<string>(OwinConstants.RequestQueryString)); }
            set { Set(OwinConstants.RequestQueryString, value.Value); }
        }

        /// <summary>
        /// Gets the query value collection parsed from owin.RequestQueryString.
        /// </summary>
        /// <returns>The query value collection parsed from owin.RequestQueryString.</returns>
        public virtual IReadableStringCollection Query
        {
            get { return new ReadableStringCollection(OwinHelpers.GetQuery(this)); }
        }

        /// <summary>
        /// Gets the uniform resource identifier (URI) associated with the request.
        /// </summary>
        /// <returns>The uniform resource identifier (URI) associated with the request.</returns>
        public virtual Uri Uri
        {
            get { return new Uri(Scheme + Uri.SchemeDelimiter + Host + PathBase + Path + QueryString); }
        }

        /// <summary>
        /// Gets or set the owin.RequestProtocol.
        /// </summary>
        /// <returns>The owin.RequestProtocol.</returns>
        public virtual string Protocol
        {
            get { return Get<string>(OwinConstants.RequestProtocol); }
            set { Set(OwinConstants.RequestProtocol, value); }
        }

        /// <summary>
        /// Gets the request headers.
        /// </summary>
        /// <returns>The request headers.</returns>
        public virtual IHeaderDictionary Headers
        {
            get { return new HeaderDictionary(RawHeaders); }
        }

        private IDictionary<string, string[]> RawHeaders
        {
            get { return Get<IDictionary<string, string[]>>(OwinConstants.RequestHeaders); }
        }

        /// <summary>
        /// Gets the collection of Cookies for this request.
        /// </summary>
        /// <returns>The collection of Cookies for this request.</returns>
        public RequestCookieCollection Cookies
        {
            get { return new RequestCookieCollection(OwinHelpers.GetCookies(this)); }
        }

        /// <summary>
        /// Gets or sets the Content-Type header.
        /// </summary>
        /// <returns>The Content-Type header.</returns>
        public virtual string ContentType
        {
            get { return OwinHelpers.GetHeader(RawHeaders, Constants.Headers.ContentType); }
            set { OwinHelpers.SetHeader(RawHeaders, Constants.Headers.ContentType, value); }
        }

        /// <summary>
        /// Gets or sets the Cache-Control header.
        /// </summary>
        /// <returns>The Cache-Control header.</returns>
        public virtual string CacheControl
        {
            get { return OwinHelpers.GetHeader(RawHeaders, Constants.Headers.CacheControl); }
            set { OwinHelpers.SetHeader(RawHeaders, Constants.Headers.CacheControl, value); }
        }

        /// <summary>
        /// Gets or sets the Media-Type header.
        /// </summary>
        /// <returns>The Media-Type header.</returns>
        public virtual string MediaType
        {
            get { return OwinHelpers.GetHeader(RawHeaders, Constants.Headers.MediaType); }
            set { OwinHelpers.SetHeader(RawHeaders, Constants.Headers.MediaType, value); }
        }

        /// <summary>
        /// Gets or set the Accept header.
        /// </summary>
        /// <returns>The Accept header.</returns>
        public virtual string Accept
        {
            get { return OwinHelpers.GetHeader(RawHeaders, Constants.Headers.Accept); }
            set { OwinHelpers.SetHeader(RawHeaders, Constants.Headers.Accept, value); }
        }

        /// <summary>
        /// Gets or set the owin.RequestBody Stream.
        /// </summary>
        /// <returns>The owin.RequestBody Stream.</returns>
        public virtual Stream Body
        {
            get { return Get<Stream>(OwinConstants.RequestBody); }
            set { Set(OwinConstants.RequestBody, value); }
        }

        /// <summary>
        /// Gets or sets the cancellation token for the request.
        /// </summary>
        /// <returns>The cancellation token for the request.</returns>
        public virtual CancellationToken CallCancelled
        {
            get { return Get<CancellationToken>(OwinConstants.CallCancelled); }
            set { Set(OwinConstants.CallCancelled, value); }
        }

        /// <summary>
        /// Gets or set the server.LocalIpAddress.
        /// </summary>
        /// <returns>The server.LocalIpAddress.</returns>
        public virtual string LocalIpAddress
        {
            get { return Get<string>(OwinConstants.CommonKeys.LocalIpAddress); }
            set { Set(OwinConstants.CommonKeys.LocalIpAddress, value); }
        }

        /// <summary>
        /// Gets or set the server.LocalPort.
        /// </summary>
        /// <returns>The server.LocalPort.</returns>
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
        /// Gets or set the server.RemoteIpAddress.
        /// </summary>
        /// <returns>The server.RemoteIpAddress.</returns>
        public virtual string RemoteIpAddress
        {
            get { return Get<string>(OwinConstants.CommonKeys.RemoteIpAddress); }
            set { Set(OwinConstants.CommonKeys.RemoteIpAddress, value); }
        }

        /// <summary>
        /// Gets or set the server.RemotePort.
        /// </summary>
        /// <returns>The server.RemotePort.</returns>
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
        /// Gets or set the server.User.
        /// </summary>
        /// <returns>The server.User.</returns>
        public virtual IPrincipal User
        {
            get { return Get<IPrincipal>(OwinConstants.Security.User); }
            set { Set(OwinConstants.Security.User, value); }
        }

        /// <summary>
        /// Asynchronously reads and parses the request body as a form.
        /// </summary>
        /// <returns>The parsed form data.</returns>
        public async Task<IFormCollection> ReadFormAsync()
        {
            var form = Get<IFormCollection>("Microsoft.Owin.Form#collection");
            if (form == null)
            {
                string text;
                using (var reader = new StreamReader(Body))
                {
                    text = await reader.ReadToEndAsync();
                }
                form = OwinHelpers.GetForm(text);
                Set("Microsoft.Owin.Form#collection", form);
            }

            return form;
        }

        /// <summary>
        /// Gets a value from the OWIN environment, or returns default(T) if not present.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value with the specified key or the default(T) if not present.</returns>
        public virtual T Get<T>(string key)
        {
            object value;
            return Environment.TryGetValue(key, out value) ? (T)value : default(T);
        }

        /// <summary>
        /// Sets the given key and value in the OWIN environment.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key of the value to set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>This instance.</returns>
        public virtual IOwinRequest Set<T>(string key, T value)
        {
            Environment[key] = value;
            return this;
        }
    }
}
