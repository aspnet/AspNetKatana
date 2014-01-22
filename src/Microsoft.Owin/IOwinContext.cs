// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Owin.Security;

namespace Microsoft.Owin
{
    /// <summary>
    /// This wraps OWIN environment dictionary and provides strongly typed accessors.
    /// </summary>
    public interface IOwinContext
    {
        /// <summary>
        /// Gets a wrapper exposing request specific properties.
        /// </summary>
        /// <returns>A wrapper exposing request specific properties.</returns>
        IOwinRequest Request { get; }

        /// <summary>
        /// Gets a wrapper exposing response specific properties.
        /// </summary>
        /// <returns>A wrapper exposing response specific properties.</returns>
        IOwinResponse Response { get; }

        /// <summary>
        /// Gets the Authentication middleware functionality available on the current request.
        /// </summary>
        /// <returns>The authentication middleware functionality available on the current request.</returns>
        IAuthenticationManager Authentication { get; }

        /// <summary>
        /// Gets the OWIN environment.
        /// </summary>
        /// <returns>The OWIN environment.</returns>
        IDictionary<string, object> Environment { get; }

        /// <summary>
        /// Gets or sets the host.TraceOutput environment value.
        /// </summary>
        /// <returns>The host.TraceOutput TextWriter.</returns>
        TextWriter TraceOutput { get; set; }

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
        IOwinContext Set<T>(string key, T value);
    }
}
