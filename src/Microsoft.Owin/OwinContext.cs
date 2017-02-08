// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Owin.Security;

namespace Microsoft.Owin
{
    /// <summary>
    /// This wraps OWIN environment dictionary and provides strongly typed accessors.
    /// </summary>
    public class OwinContext : IOwinContext
    {
        /// <summary>
        /// Create a new context with only request and response header collections.
        /// </summary>
        public OwinContext()
        {
            IDictionary<string, object> environment = new Dictionary<string, object>(StringComparer.Ordinal);
            environment[OwinConstants.RequestHeaders] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            environment[OwinConstants.ResponseHeaders] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            Environment = environment;
            Request = new OwinRequest(environment);
            Response = new OwinResponse(environment);
        }

        /// <summary>
        /// Create a new wrapper.
        /// </summary>
        /// <param name="environment">OWIN environment dictionary which stores state information about the request, response and relevant server state.</param>
        public OwinContext(IDictionary<string, object> environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            Environment = environment;
            Request = new OwinRequest(environment);
            Response = new OwinResponse(environment);
        }

        /// <summary>
        /// Gets a wrapper exposing request specific properties.
        /// </summary>
        /// <returns>A wrapper exposing request specific properties.</returns>
        public virtual IOwinRequest Request { get; private set; }

        /// <summary>
        /// Gets a wrapper exposing response specific properties.
        /// </summary>
        /// <returns>A wrapper exposing response specific properties.</returns>
        public virtual IOwinResponse Response { get; private set; }

        /// <summary>
        /// Gets the Authentication middleware functionality available on the current request.
        /// </summary>
        /// <returns>The authentication middleware functionality available on the current request.</returns>
        public IAuthenticationManager Authentication
        {
            get { return new AuthenticationManager(this); }
        }

        /// <summary>
        /// Gets the OWIN environment.
        /// </summary>
        /// <returns>The OWIN environment.</returns>
        public virtual IDictionary<string, object> Environment { get; private set; }

        /// <summary>
        /// Gets or sets the host.TraceOutput environment value.
        /// </summary>
        /// <returns>The host.TraceOutput TextWriter.</returns>
        public virtual TextWriter TraceOutput
        {
            get { return Get<TextWriter>(OwinConstants.CommonKeys.TraceOutput); }
            set { Set<TextWriter>(OwinConstants.CommonKeys.TraceOutput, value); }
        }

        /// <summary>
        /// Gets a value from the OWIN environment, or returns default(T) if not present.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value with the specified key or the default(T) if not present.</returns>
        public virtual T Get<T>(string key)
        {
            object value;
            return Environment.TryGetValue(key, out value) ? (T)value : default(T);
        }

        /// <summary>
        /// Sets the given key and value in the OWIN environment.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key of the value to set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>This instance.</returns>
        public virtual IOwinContext Set<T>(string key, T value)
        {
            Environment[key] = value;
            return this;
        }
    }
}
