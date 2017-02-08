// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin
{
    /// <summary>
    /// This wraps OWIN environment dictionary and provides strongly typed accessors.
    /// </summary>
    public interface IOwinResponse
    {
        /// <summary>
        /// Gets the OWIN environment.
        /// </summary>
        /// <returns>The OWIN environment.</returns>
        IDictionary<string, object> Environment { get; }

        /// <summary>
        /// Gets the request context.
        /// </summary>
        /// <returns>The request context.</returns>
        IOwinContext Context { get; }

        /// <summary>
        /// Gets or sets the optional owin.ResponseStatusCode.
        /// </summary>
        /// <returns>The optional owin.ResponseStatusCode, or 200 if not set.</returns>
        int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the the optional owin.ResponseReasonPhrase.
        /// </summary>
        /// <returns>The the optional owin.ResponseReasonPhrase.</returns>
        string ReasonPhrase { get; set; }

        /// <summary>
        /// Gets or sets the owin.ResponseProtocol.
        /// </summary>
        /// <returns>The owin.ResponseProtocol.</returns>
        string Protocol { get; set; }

        /// <summary>
        /// Gets the response header collection.
        /// </summary>
        /// <returns>The response header collection.</returns>
        IHeaderDictionary Headers { get; }

        /// <summary>
        /// Gets a collection used to manipulate the Set-Cookie header.
        /// </summary>
        /// <returns>A collection used to manipulate the Set-Cookie header.</returns>
        ResponseCookieCollection Cookies { get; }

        /// <summary>
        /// Gets or sets the Content-Length header.
        /// </summary>
        /// <returns>The Content-Length header.</returns>
        long? ContentLength { get; set; }

        /// <summary>
        /// Gets or sets the Content-Type header.
        /// </summary>
        /// <returns>The Content-Type header.</returns>
        string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the Expires header.
        /// </summary>
        /// <returns>The Expires header.</returns>
        DateTimeOffset? Expires { get; set; }

        /// <summary>
        /// Gets or sets the E-Tag header.
        /// </summary>
        /// <returns>The E-Tag header.</returns>
        string ETag { get; set; }

        /// <summary>
        /// Gets or sets the owin.ResponseBody Stream.
        /// </summary>
        /// <returns>The owin.ResponseBody Stream.</returns>
        Stream Body { get; set; }

        /// <summary>
        /// Registers for an event that fires when the response headers are sent.
        /// </summary>
        /// <param name="callback">The callback method.</param>
        /// <param name="state">The callback state.</param>
        void OnSendingHeaders(Action<object> callback, object state);

        /// <summary>
        /// Sets a 302 response status code and the Location header.
        /// </summary>
        /// <param name="location">The location where to redirect the client.</param>
        void Redirect(string location);

        /// <summary>
        /// Writes the given text to the response body stream using UTF-8.
        /// </summary>
        /// <param name="text">The response data.</param>
        void Write(string text);

        /// <summary>
        /// Writes the given bytes to the response body stream.
        /// </summary>
        /// <param name="data">The response data.</param>
        void Write(byte[] data);

        /// <summary>
        /// Writes the given bytes to the response body stream.
        /// </summary>
        /// <param name="data">The response data.</param>
        /// <param name="offset">The zero-based byte offset in the <paramref name="data" /> parameter at which to begin copying bytes.</param>
        /// <param name="count">The number of bytes to write.</param>
        void Write(byte[] data, int offset, int count);

        /// <summary>
        /// Asynchronously writes the given text to the response body stream using UTF-8.
        /// </summary>
        /// <param name="text">The response data.</param>
        /// <returns>A Task tracking the state of the write operation.</returns>
        Task WriteAsync(string text);

        /// <summary>
        /// Asynchronously writes the given text to the response body stream using UTF-8.
        /// </summary>
        /// <param name="text">The response data.</param>
        /// <param name="token">A token used to indicate cancellation.</param>
        /// <returns>A Task tracking the state of the write operation.</returns>
        Task WriteAsync(string text, CancellationToken token);

        /// <summary>
        /// Asynchronously writes the given bytes to the response body stream.
        /// </summary>
        /// <param name="data">The response data.</param>
        /// <returns>A Task tracking the state of the write operation.</returns>
        Task WriteAsync(byte[] data);

        /// <summary>
        /// Asynchronously writes the given bytes to the response body stream.
        /// </summary>
        /// <param name="data">The response data.</param>
        /// <param name="token">A token used to indicate cancellation.</param>
        /// <returns>A Task tracking the state of the write operation.</returns>
        Task WriteAsync(byte[] data, CancellationToken token);

        /// <summary>
        /// Asynchronously writes the given bytes to the response body stream.
        /// </summary>
        /// <param name="data">The response data.</param>
        /// <param name="offset">The zero-based byte offset in the <paramref name="data" /> parameter at which to begin copying bytes.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <param name="token">A token used to indicate cancellation.</param>
        /// <returns>A Task tracking the state of the write operation.</returns>
        Task WriteAsync(byte[] data, int offset, int count, CancellationToken token);

        /// <summary>
        /// Gets a value from the OWIN environment, or returns default(T) if not present.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value with the specified key or the default(T) if not present.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Re-evaluate later.")]
        T Get<T>(string key);

        /// <summary>
        /// Sets the given key and value in the OWIN environment.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key of the value to set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>This instance.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set", Justification = "Re-evaluate later.")]
        IOwinResponse Set<T>(string key, T value);
    }
}
