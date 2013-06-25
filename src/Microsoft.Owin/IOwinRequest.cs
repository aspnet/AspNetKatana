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
    public interface IOwinRequest
    {
        IDictionary<string, object> Environment { get; }

        IOwinContext Context { get; }

        // Core OWIN spec fields:
        Stream Body { get; set; }
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Cancelled", Justification = "In OWIN spec.")]
        CancellationToken CallCancelled { get; set; }
        string Method { get; set; }
        string Path { get; set; }
        string PathBase { get; set; }
        string Protocol { get; set; }
        string QueryString { get; set; }
        string Scheme { get; set; }

        // Common server fields:
        string LocalIpAddress { get; set; }
        int? LocalPort { get; set; }
        string RemoteIpAddress { get; set; }
        int? RemotePort { get; set; }

        // Common headers:
        string ContentType { get; set; }
        string CacheControl { get; set; }
        string MediaType { get; set; }
        string Accept { get; set; }
        string Host { get; set; }

        bool IsSecure { get; }
        Uri Uri { get; }
        IPrincipal User { get; set; }

        // Collections:
        IHeaderDictionary Headers { get; }
        IReadableStringCollection Query { get; } // Read Only parsed collection
        RequestCookieCollection Cookies { get; }

#if !NET40
        IAuthenticationManager Authentication { get; }
        Task<IFormCollection> ReadFormAsync();
#endif

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Re-evaluate later.")]
        T Get<T>(string key);
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set", Justification = "Re-evaluate later.")]
        IOwinRequest Set<T>(string key, T value);

        // IAuthenticationManager, User?
    }
}
