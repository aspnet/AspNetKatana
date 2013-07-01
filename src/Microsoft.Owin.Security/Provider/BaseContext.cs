// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Owin.Security.Provider
{
    public abstract class BaseContext
    {
        protected BaseContext(IDictionary<string, object> environment)
        {
            Environment = environment;
        }

        public IDictionary<string, object> Environment { get; private set; }
    }
}
