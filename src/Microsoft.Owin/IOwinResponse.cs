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
    public interface IOwinResponse
    {
        IDictionary<string, object> Environment { get; }

        IOwinContext Context { get; }

        Stream Body { get; set; }
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Cancelled", Justification = "In OWIN spec.")]
        CancellationToken CallCancelled { get; set; }
        int StatusCode { get; set; } // Default to 200 if not defined in the env.
        string ReasonPhrase { get; set; }
        string Protocol { get; set; }

        // Collections:
        IHeaderDictionary Headers { get; }
        ResponseCookieCollection Cookies { get; } // Write-only helper

        // Common headers:
        long? ContentLength { get; set; }
        string ContentType { get; set; }
        DateTimeOffset? Expires { get; set; }
        string ETag { get; set; }

#if !NET40
        IAuthenticationManager Authentication { get; }
#endif

        void Write(string text);
        void Write(byte[] data);
        void Write(byte[] data, int offset, int count);
        Task WriteAsync(string text);
        Task WriteAsync(string text, CancellationToken token);
        Task WriteAsync(byte[] data);
        Task WriteAsync(byte[] data, CancellationToken token);
        Task WriteAsync(byte[] data, int offset, int count, CancellationToken token);

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Re-evaluate later.")]
        T Get<T>(string key);
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set", Justification = "Re-evaluate later.")]
        IOwinResponse Set<T>(string key, T value);

        void OnSendingHeaders(Action<object> callback, object state);
        void Redirect(string location);
    }
}
