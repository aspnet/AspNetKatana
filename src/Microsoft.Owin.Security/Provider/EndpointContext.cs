// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Owin.Security.Provider
{
    public abstract class EndpointContext : BaseContext
    {
        protected EndpointContext(IDictionary<string, object> environment) : base(environment)
        {
        }

        public bool IsRequestCompleted { get; private set; }

        public void RequestCompleted()
        {
            IsRequestCompleted = true;
        }
    }
}
