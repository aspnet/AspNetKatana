// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#if !NET40
using Microsoft.Owin.Security;
#endif

namespace Microsoft.Owin
{
    /// <summary>
    /// This wraps OWIN environment dictionary and provides strongly typed accessors.
    /// </summary>
    public interface IOwinResponse
    {
        /// <summary>
        /// The wrapped OWIN environment.
        /// </summary>
        IDictionary<string, object> Environment { get; }

        /// <summary>
        /// 
        /// </summary>
        IOwinContext Context { get; }

        /// <summary>
        /// The optional owin.ResponseStatusCode.
        /// </summary>
        int StatusCode { get; set; } // Default to 200 if not defined in the env.

        /// <summary>
        /// The optional owin.ResponseReasonPhrase.
        /// </summary>
        string ReasonPhrase { get; set; }

        /// <summary>
        /// owin.ResponseProtocol
        /// </summary>
        string Protocol { get; set; }

        /// <summary>
        /// owin.ResponseHeaders in a wrapper
        /// </summary>
        IHeaderDictionary Headers { get; }

        /// <summary>
        /// The Set-Cookie header in a wrapper
        /// </summary>
        ResponseCookieCollection Cookies { get; } // Write-only helper

        /// <summary>
        /// The Content-Length header
        /// </summary>
        long? ContentLength { get; set; }

        /// <summary>
        /// The Content-Type response header.
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// The Expires header
        /// </summary>
        DateTimeOffset? Expires { get; set; }

        /// <summary>
        /// The E-Tag header
        /// </summary>
        string ETag { get; set; }

        /// <summary>
        /// The owin.ResponseBody Stream.
        /// </summary>
        Stream Body { get; set; }

#if !NET40
        /// <summary>
        /// Access the Authentication middleware functionality available on the current request.
        /// </summary>
        IAuthenticationManager Authentication { get; }
#endif

        /// <summary>
        /// Registers for an event that fires when the response headers are sent.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        void OnSendingHeaders(Action<object> callback, object state);

        /// <summary>
        /// Sets a 302 response status code and the Location header.
        /// </summary>
        /// <param name="location"></param>
        void Redirect(string location);

        /// <summary>
        /// Writes the given text to the response stream using UTF-8
        /// </summary>
        /// <param name="text"></param>
        void Write(string text);

        /// <summary>
        /// Writes the given bytes to the response stream
        /// </summary>
        /// <param name="data"></param>
        void Write(byte[] data);

        /// <summary>
        /// Writes the given bytes to the response stream
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        void Write(byte[] data, int offset, int count);

        /// <summary>
        /// Writes the given text to the response stream using UTF-8
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        Task WriteAsync(string text);

        /// <summary>
        /// Writes the given text to the response stream using UTF-8
        /// </summary>
        /// <param name="text"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task WriteAsync(string text, CancellationToken token);

        /// <summary>
        /// Writes the given bytes to the response stream
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task WriteAsync(byte[] data);

        /// <summary>
        /// Writes the given bytes to the response stream
        /// </summary>
        /// <param name="data"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task WriteAsync(byte[] data, CancellationToken token);

        /// <summary>
        /// Writes the given bytes to the response stream
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task WriteAsync(byte[] data, int offset, int count, CancellationToken token);

        /// <summary>
        /// Gets a value from the OWIN environment, or returns default(T) if not present.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Re-evaluate later.")]
        T Get<T>(string key);

        /// <summary>
        /// Sets the given key and value in the OWIN environment.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set", Justification = "Re-evaluate later.")]
        IOwinResponse Set<T>(string key, T value);
    }
}
