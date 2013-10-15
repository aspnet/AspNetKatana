// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Infrastructure;

namespace Microsoft.Owin
{
    /// <summary>
    /// This wraps OWIN environment dictionary and provides strongly typed accessors.
    /// </summary>
    public partial class OwinResponse : IOwinResponse
    {
        /// <summary>
        /// Create a new context with only request and response header collections.
        /// </summary>
        public OwinResponse()
        {
            IDictionary<string, object> environment = new Dictionary<string, object>(StringComparer.Ordinal);
            environment[OwinConstants.RequestHeaders] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            environment[OwinConstants.ResponseHeaders] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            Environment = environment;
        }

        /// <summary>
        /// Creates a new environment wrapper exposing response properties.
        /// </summary>
        /// <param name="environment"></param>
        public OwinResponse(IDictionary<string, object> environment)
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
        /// Gets or sets the optional owin.ResponseStatusCode.
        /// </summary>
        /// <returns>The optional owin.ResponseStatusCode, or 200 if not set.</returns>
        public virtual int StatusCode
        {
            get { return Get<int>(OwinConstants.ResponseStatusCode, 200); }
            set { Set(OwinConstants.ResponseStatusCode, value); }
        }

        /// <summary>
        /// Gets or sets the the optional owin.ResponseReasonPhrase.
        /// </summary>
        /// <returns>The the optional owin.ResponseReasonPhrase.</returns>
        public virtual string ReasonPhrase
        {
            get { return Get<string>(OwinConstants.ResponseReasonPhrase); }
            set { Set(OwinConstants.ResponseReasonPhrase, value); }
        }

        /// <summary>
        /// Gets or sets the owin.ResponseProtocol.
        /// </summary>
        /// <returns>The owin.ResponseProtocol.</returns>
        public virtual string Protocol
        {
            get { return Get<string>(OwinConstants.ResponseProtocol); }
            set { Set(OwinConstants.ResponseProtocol, value); }
        }

        /// <summary>
        /// Gets the response header collection.
        /// </summary>
        /// <returns>The response header collection.</returns>
        public virtual IHeaderDictionary Headers
        {
            get { return new HeaderDictionary(RawHeaders); }
        }

        private IDictionary<string, string[]> RawHeaders
        {
            get { return Get<IDictionary<string, string[]>>(OwinConstants.ResponseHeaders); }
        }

        /// <summary>
        /// Gets a collection used to manipulate the Set-Cookie header.
        /// </summary>
        /// <returns>A collection used to manipulate the Set-Cookie header.</returns>
        public virtual ResponseCookieCollection Cookies
        {
            get { return new ResponseCookieCollection(Headers); }
        }

        /// <summary>
        /// Gets or sets the Content-Length header.
        /// </summary>
        /// <returns>The Content-Length header.</returns>
        public virtual long? ContentLength
        {
            get
            {
                long value;
                if (long.TryParse(OwinHelpers.GetHeader(RawHeaders, Constants.Headers.ContentLength), out value))
                {
                    return value;
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    OwinHelpers.SetHeader(RawHeaders, Constants.Headers.ContentLength,
                        value.Value.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    RawHeaders.Remove(Constants.Headers.ContentLength);
                }
            }
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
        /// Gets or sets the Expires header.
        /// </summary>
        /// <returns>The Expires header.</returns>
        public virtual DateTimeOffset? Expires
        {
            get
            {
                DateTimeOffset value;
                if (DateTimeOffset.TryParse(OwinHelpers.GetHeader(RawHeaders, Constants.Headers.Expires),
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out value))
                {
                    return value;
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    OwinHelpers.SetHeader(RawHeaders, Constants.Headers.Expires,
                        value.Value.ToString(Constants.HttpDateFormat, CultureInfo.InvariantCulture));
                }
                else
                {
                    RawHeaders.Remove(Constants.Headers.Expires);
                }
            }
        }

        /// <summary>
        /// Gets or sets the E-Tag header.
        /// </summary>
        /// <returns>The E-Tag header.</returns>
        public virtual string ETag
        {
            get { return OwinHelpers.GetHeader(RawHeaders, Constants.Headers.ETag); }
            set { OwinHelpers.SetHeader(RawHeaders, Constants.Headers.ETag, value); }
        }

        /// <summary>
        /// Gets or sets the owin.ResponseBody Stream.
        /// </summary>
        /// <returns>The owin.ResponseBody Stream.</returns>
        public virtual Stream Body
        {
            get { return Get<Stream>(OwinConstants.ResponseBody); }
            set { Set(OwinConstants.ResponseBody, value); }
        }

        /// <summary>
        /// Registers for an event that fires when the response headers are sent.
        /// </summary>
        /// <param name="callback">The callback method.</param>
        /// <param name="state">The callback state.</param>
        public virtual void OnSendingHeaders(Action<object> callback, object state)
        {
            var onSendingHeaders = Get<Action<Action<object>, object>>(OwinConstants.CommonKeys.OnSendingHeaders);
            if (onSendingHeaders == null)
            {
                throw new NotSupportedException(Resources.Exception_MissingOnSendingHeaders);
            }
            onSendingHeaders(callback, state);
        }

        /// <summary>
        /// Sets a 302 response status code and the Location header.
        /// </summary>
        /// <param name="location">The location where to redirect the client.</param>
        public virtual void Redirect(string location)
        {
            StatusCode = 302;
            OwinHelpers.SetHeader(RawHeaders, Constants.Headers.Location, location);
        }

        /// <summary>
        /// Writes the given text to the response body stream using UTF-8.
        /// </summary>
        /// <param name="text">The response data.</param>
        public virtual void Write(string text)
        {
            Write(Encoding.UTF8.GetBytes(text));
        }

        /// <summary>
        /// Writes the given bytes to the response body stream.
        /// </summary>
        /// <param name="data">The response data.</param>
        public virtual void Write(byte[] data)
        {
            Write(data, 0, data == null ? 0 : data.Length);
        }

        /// <summary>
        /// Writes the given bytes to the response body stream.
        /// </summary>
        /// <param name="data">The response data.</param>
        /// <param name="offset">The zero-based byte offset in the <paramref name="data" /> parameter at which to begin copying bytes.</param>
        /// <param name="count">The number of bytes to write.</param>
        public virtual void Write(byte[] data, int offset, int count)
        {
            Body.Write(data, offset, count);
        }

        /// <summary>
        /// Asynchronously writes the given text to the response body stream using UTF-8.
        /// </summary>
        /// <param name="text">The response data.</param>
        /// <returns>A Task tracking the state of the write operation.</returns>
        public virtual Task WriteAsync(string text)
        {
            return WriteAsync(text, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously writes the given text to the response body stream using UTF-8.
        /// </summary>
        /// <param name="text">The response data.</param>
        /// <param name="token">A token used to indicate cancellation.</param>
        /// <returns>A Task tracking the state of the write operation.</returns>
        public virtual Task WriteAsync(string text, CancellationToken token)
        {
            return WriteAsync(Encoding.UTF8.GetBytes(text), token);
        }

        /// <summary>
        /// Asynchronously writes the given bytes to the response body stream.
        /// </summary>
        /// <param name="data">The response data.</param>
        /// <returns>A Task tracking the state of the write operation.</returns>
        public virtual Task WriteAsync(byte[] data)
        {
            return WriteAsync(data, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously writes the given bytes to the response body stream.
        /// </summary>
        /// <param name="data">The response data.</param>
        /// <param name="token">A token used to indicate cancellation.</param>
        /// <returns>A Task tracking the state of the write operation.</returns>
        public virtual Task WriteAsync(byte[] data, CancellationToken token)
        {
            return WriteAsync(data, 0, data == null ? 0 : data.Length, token);
        }

        /// <summary>
        /// Asynchronously writes the given bytes to the response body stream.
        /// </summary>
        /// <param name="data">The response data.</param>
        /// <param name="offset">The zero-based byte offset in the <paramref name="data" /> parameter at which to begin copying bytes.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <param name="token">A token used to indicate cancellation.</param>
        /// <returns>A Task tracking the state of the write operation.</returns>
        public virtual Task WriteAsync(byte[] data, int offset, int count, CancellationToken token)
        {
#if NET40
            Stream body = Body;
            return Task.Factory.FromAsync(body.BeginWrite, body.EndWrite, data, offset, count, token);
#else
            return Body.WriteAsync(data, offset, count, token);
#endif
        }

        /// <summary>
        /// Gets a value from the OWIN environment, or returns default(T) if not present.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value with the specified key or the default(T) if not present.</returns>
        public virtual T Get<T>(string key)
        {
            return Get(key, default(T));
        }

        private T Get<T>(string key, T fallback)
        {
            object value;
            return Environment.TryGetValue(key, out value) ? (T)value : fallback;
        }

        /// <summary>
        /// Sets the given key and value in the OWIN environment.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key of the value to set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>This instance.</returns>
        public virtual IOwinResponse Set<T>(string key, T value)
        {
            Environment[key] = value;
            return this;
        }
    }
}
