// <copyright file="OwinResponse.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Infrastructure;
#if !NET40
using Microsoft.Owin.Security;
#endif

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
        /// The wrapped OWIN environment.
        /// </summary>
        public virtual IDictionary<string, object> Environment
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IOwinContext Context
        {
            get { return new OwinContext(Environment); }
        }

        /// <summary>
        /// The optional owin.ResponseStatusCode.
        /// </summary>
        public virtual int StatusCode
        {
            get { return Get<int>(OwinConstants.ResponseStatusCode, 200); }
            set { Set(OwinConstants.ResponseStatusCode, value); }
        }

        /// <summary>
        /// The optional owin.ResponseReasonPhrase.
        /// </summary>
        public virtual string ReasonPhrase
        {
            get { return Get<string>(OwinConstants.ResponseReasonPhrase); }
            set { Set(OwinConstants.ResponseReasonPhrase, value); }
        }

        /// <summary>
        /// owin.ResponseProtocol
        /// </summary>
        public virtual string Protocol
        {
            get { return Get<string>(OwinConstants.ResponseProtocol); }
            set { Set(OwinConstants.ResponseProtocol, value); }
        }

        /// <summary>
        /// owin.ResponseHeaders in a wrapper
        /// </summary>
        public virtual IHeaderDictionary Headers
        {
            get { return new HeaderDictionary(RawHeaders); }
        }

        private IDictionary<string, string[]> RawHeaders
        {
            get { return Get<IDictionary<string, string[]>>(OwinConstants.ResponseHeaders); }
        }

        /// <summary>
        /// The Set-Cookie header in a wrapper
        /// </summary>
        public virtual ResponseCookieCollection Cookies
        {
            get { return new ResponseCookieCollection(Headers); }
        }

        /// <summary>
        /// The Content-Length header
        /// </summary>
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
        /// The Content-Type response header.
        /// </summary>
        public virtual string ContentType
        {
            get { return OwinHelpers.GetHeader(RawHeaders, Constants.Headers.ContentType); }
            set { OwinHelpers.SetHeader(RawHeaders, Constants.Headers.ContentType, value); }
        }

        /// <summary>
        /// The Expires header
        /// </summary>
        public virtual DateTimeOffset? Expires
        {
            get
            {
                DateTimeOffset value;
                // TODO: Format?
                if (DateTimeOffset.TryParse(OwinHelpers.GetHeader(RawHeaders, Constants.Headers.Expires), out value))
                {
                    return value;
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    // TODO: Format?
                    OwinHelpers.SetHeader(RawHeaders, Constants.Headers.Expires,
                        value.Value.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    RawHeaders.Remove(Constants.Headers.Expires);
                }
            }
        }

        /// <summary>
        /// The ETag header
        /// </summary>
        public virtual string ETag
        {
            get { return OwinHelpers.GetHeader(RawHeaders, Constants.Headers.ETag); }
            set { OwinHelpers.SetHeader(RawHeaders, Constants.Headers.ETag, value); }
        }

        /// <summary>
        /// The owin.ResponseBody Stream.
        /// </summary>
        public virtual Stream Body
        {
            get { return Get<Stream>(OwinConstants.ResponseBody); }
            set { Set(OwinConstants.ResponseBody, value); }
        }

#if !NET40
        /// <summary>
        /// Access the Authentication middleware functionality available on the current request.
        /// </summary>
        public IAuthenticationManager Authentication
        {
            get
            {
                return new AuthenticationManager(Context);
            }
        }
#endif

        /// <summary>
        /// Registers for an event that fires when the response headers are sent.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public virtual void OnSendingHeaders(Action<object> callback, object state)
        {
            Get<Action<Action<object>, object>>(OwinConstants.CommonKeys.OnSendingHeaders)(callback, state);
        }

        /// <summary>
        /// Sets a 302 response status code and the Location header.
        /// </summary>
        /// <param name="location"></param>
        public virtual void Redirect(string location)
        {
            StatusCode = 302;
            OwinHelpers.SetHeader(RawHeaders, Constants.Headers.Location, location);
        }

        /// <summary>
        /// Writes the given text to the response stream using UTF-8
        /// </summary>
        /// <param name="text"></param>
        public virtual void Write(string text)
        {
            Write(Encoding.UTF8.GetBytes(text));
        }

        /// <summary>
        /// Writes the given bytes to the response stream
        /// </summary>
        /// <param name="data"></param>
        public virtual void Write(byte[] data)
        {
            Write(data, 0, data == null ? 0 : data.Length);
        }

        /// <summary>
        /// Writes the given bytes to the response stream
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public virtual void Write(byte[] data, int offset, int count)
        {
            Body.Write(data, offset, count);
        }

        /// <summary>
        /// Writes the given text to the response stream using UTF-8
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public virtual Task WriteAsync(string text)
        {
            return WriteAsync(text, CancellationToken.None);
        }

        /// <summary>
        /// Writes the given text to the response stream using UTF-8
        /// </summary>
        /// <param name="text"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual Task WriteAsync(string text, CancellationToken token)
        {
            return WriteAsync(Encoding.UTF8.GetBytes(text), token);
        }

        /// <summary>
        /// Writes the given bytes to the response stream
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual Task WriteAsync(byte[] data)
        {
            return WriteAsync(data, CancellationToken.None);
        }

        /// <summary>
        /// Writes the given bytes to the response stream
        /// </summary>
        /// <param name="data"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual Task WriteAsync(byte[] data, CancellationToken token)
        {
            return WriteAsync(data, 0, data == null ? 0 : data.Length, token);
        }

        /// <summary>
        /// Writes the given bytes to the response stream
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="token"></param>
        /// <returns></returns>
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
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
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
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public virtual IOwinResponse Set<T>(string key, T value)
        {
            Environment[key] = value;
            return this;
        }
    }
}