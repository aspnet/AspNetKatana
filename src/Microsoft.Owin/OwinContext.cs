// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Owin
{
    /// <summary>
    /// This wraps OWIN environment dictionary and provides strongly typed accessors.
    /// </summary>
    public partial class OwinContext : IOwinContext
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
        /// <param name="environment"></param>
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
        /// A wrapper exposing request specific properties
        /// </summary>
        public virtual IOwinRequest Request { get; private set; }

        /// <summary>
        /// A wrapper exposing response specific properties
        /// </summary>
        public virtual IOwinResponse Response { get; private set; }

        /// <summary>
        /// The wrapped OWIN environment.
        /// </summary>
        public virtual IDictionary<string, object> Environment { get; private set; }

        /// <summary>
        /// Gets or sets the host.TraceOutput environment value.
        /// </summary>
        public virtual TextWriter TraceOutput
        {
            get { return Get<TextWriter>(OwinConstants.CommonKeys.TraceOutput); }
            set { Set<TextWriter>(OwinConstants.CommonKeys.TraceOutput, value); }
        }

        /// <summary>
        /// Gets a value from the OWIN environment, or returns default(T) if not present.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual T Get<T>(string key)
        {
            object value;
            return Environment.TryGetValue(key, out value) ? (T)value : default(T);
        }

        /// <summary>
        /// Sets the given key and value in the OWIN environment.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public virtual IOwinContext Set<T>(string key, T value)
        {
            Environment[key] = value;
            return this;
        }
    }
}
