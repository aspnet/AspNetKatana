// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Owin
{
    public class OwinContext : IOwinContext
    {
        public OwinContext()
        {
            IDictionary<string, object> environment = new Dictionary<string, object>(StringComparer.Ordinal);
            environment[OwinConstants.RequestHeaders] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            environment[OwinConstants.ResponseHeaders] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            Environment = environment;
            Request = new OwinRequest(environment);
            Response = new OwinResponse(environment);
        }

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

        public virtual IOwinRequest Request { get; private set; }
        public virtual IOwinResponse Response { get; private set; }

        public virtual IDictionary<string, object> Environment { get; private set; }

        public virtual T Get<T>(string key)
        {
            object value;
            return Environment.TryGetValue(key, out value) ? (T)value : default(T);
        }

        public virtual IOwinContext Set<T>(string key, T value)
        {
            Environment[key] = value;
            return this;
        }
    }
}
