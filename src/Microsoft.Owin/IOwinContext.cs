// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#if !NET40
using Microsoft.Owin.Security;
#endif

namespace Microsoft.Owin
{
    /// <summary>
    /// This wraps OWIN environment dictionary and provides strongly typed accessors.
    /// </summary>
    public interface IOwinContext
    {
        /// <summary>
        /// A wrapper exposing request specific properties
        /// </summary>
        IOwinRequest Request { get; }

        /// <summary>
        /// A wrapper exposing response specific properties
        /// </summary>
        IOwinResponse Response { get; }

        /// <summary>
        /// The wrapped OWIN environment.
        /// </summary>
        IDictionary<string, object> Environment { get; }

#if !NET40
        /// <summary>
        /// Access the Authentication middleware functionality available on the current request.
        /// </summary>
        IAuthenticationManager Authentication { get; }
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
        IOwinContext Set<T>(string key, T value);
    }
}
