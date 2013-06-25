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
        /// The Content-Type response header.
        /// </summary>
        public virtual string ContentType
        {
            get { return OwinHelpers.GetHeader(RawHeaders, Constants.Headers.ContentType); }
            set { OwinHelpers.SetHeader(RawHeaders, Constants.Headers.ContentType, value); }
        }

        /// <summary>
        /// The owin.ResponseBody Stream.
        /// </summary>
        public virtual Stream Body
        {
            get { return Get<Stream>(OwinConstants.ResponseBody); }
            set { Set(OwinConstants.ResponseBody, value); }
        }

        public virtual IOwinContext Context
        {
            get { return new OwinContext(Environment); }
        }

        public virtual CancellationToken CallCancelled
        {
            get { return Get<CancellationToken>(OwinConstants.CallCancelled); }
            set { Set(OwinConstants.CallCancelled, value); }
        }

        public virtual string Protocol
        {
            get { return Get<string>(OwinConstants.ResponseProtocol); }
            set { Set(OwinConstants.ResponseProtocol, value); }
        }

        public virtual IHeaderDictionary Headers
        {
            get { return new HeaderDictionary(RawHeaders); }
        }

        internal virtual IDictionary<string, string[]> RawHeaders
        {
            get { return Get<IDictionary<string, string[]>>(OwinConstants.ResponseHeaders); }
        }

        public virtual ResponseCookieCollection Cookies
        {
            get { return new ResponseCookieCollection(Headers); }
        }

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

        public virtual string ETag
        {
            get { return OwinHelpers.GetHeader(RawHeaders, Constants.Headers.ETag); }
            set { OwinHelpers.SetHeader(RawHeaders, Constants.Headers.ETag, value); }
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
        /// Sets a 302 response status code and the Location header.
        /// </summary>
        /// <param name="location"></param>
        public virtual void Redirect(string location)
        {
            StatusCode = 302;
            OwinHelpers.SetHeader(RawHeaders, Constants.Headers.Location, location);
        }

        public virtual void Write(byte[] data)
        {
            throw new NotImplementedException();
        }

        public virtual void Write(byte[] data, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public virtual void Write(string text)
        {
            throw new NotImplementedException();
        }

        public virtual Task WriteAsync(byte[] data, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public virtual Task WriteAsync(byte[] data, int offset, int count, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public virtual Task WriteAsync(string text, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public virtual T Get<T>(string key)
        {
            return Get(key, default(T));
        }

        private T Get<T>(string key, T fallback)
        {
            object value;
            return Environment.TryGetValue(key, out value) ? (T)value : fallback;
        }

        public virtual IOwinResponse Set<T>(string key, T value)
        {
            Environment[key] = value;
            return this;
        }

        /// <summary>
        /// Registers for an event that fires when the response headers are sent.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public virtual void OnSendingHeaders(Action<object> callback, object state)
        {
            Get<Action<Action<object>, object>>(OwinConstants.CommonKeys.OnSendingHeaders)(callback, state);
        }
    }
}