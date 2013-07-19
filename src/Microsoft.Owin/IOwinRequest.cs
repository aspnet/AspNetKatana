// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Principal;
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
    public interface IOwinRequest
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
        /// The HTTP method/verb, e.g. GET, POST, etc..
        /// </summary>
        string Method { get; set; }

        /// <summary>
        /// The HTTP request scheme (e.g. http or https) from owin.RequestScheme.
        /// </summary>
        string Scheme { get; set; }

        /// <summary>
        /// Returns true if the owin.RequestScheme is https.
        /// </summary>
        bool IsSecure { get; }

        /// <summary>
        /// The request host, taken from the Host request header in owin.RequestHeaders.
        /// May include the port.
        /// </summary>
        string Host { get; set; }

        /// <summary>
        /// See owin.RequestPathBase.
        /// </summary>
        string PathBase { get; set; }

        /// <summary>
        /// The request path from owin.RequestPath.
        /// </summary>
        string Path { get; set; }

        /// <summary>
        /// The query string from owin.RequestQueryString.
        /// </summary>
        string QueryString { get; set; }

        /// <summary>
        /// owin.RequestQueryString parsed into a collection
        /// </summary>
        IReadableStringCollection Query { get; } // Read Only parsed collection

        /// <summary>
        /// A Uri with the combine parts of owin.RequestScheme, the Host header, owin.RequestPathBase, owin.RequestPath, and owin.RequestQueryString.
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// owin.RequestProtocol
        /// </summary>
        string Protocol { get; set; }

        /// <summary>
        /// owin.RequestHeaders in a wrapper
        /// </summary>
        IHeaderDictionary Headers { get; }

        /// <summary>
        /// The Cookie header parsed into a collection
        /// </summary>
        RequestCookieCollection Cookies { get; }

        /// <summary>
        /// The Content-Type header
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// The Cache-Control header
        /// </summary>
        string CacheControl { get; set; }

        /// <summary>
        /// The Media-Type header
        /// </summary>
        string MediaType { get; set; }

        /// <summary>
        /// The Accept header
        /// </summary>
        string Accept { get; set; }

        /// <summary>
        /// The owin.RequestBody Stream.
        /// </summary>
        Stream Body { get; set; }

        /// <summary>
        /// owin.CallCancelled
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Cancelled", Justification = "In OWIN spec.")]
        CancellationToken CallCancelled { get; set; }

        /// <summary>
        /// server.LocalIpAddress
        /// </summary>
        string LocalIpAddress { get; set; }

        /// <summary>
        /// server.LocalPort
        /// </summary>
        int? LocalPort { get; set; }

        /// <summary>
        /// server.RemoteIpAddress
        /// </summary>
        string RemoteIpAddress { get; set; }

        /// <summary>
        /// server.RemotePort
        /// </summary>
        int? RemotePort { get; set; }

        /// <summary>
        /// server.User.
        /// </summary>
        IPrincipal User { get; set; }

#if !NET40
        /// <summary>
        /// Parses the request body as a form
        /// </summary>
        Task<IFormCollection> ReadFormAsync();
#endif
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
        IOwinRequest Set<T>(string key, T value);
    }
}
