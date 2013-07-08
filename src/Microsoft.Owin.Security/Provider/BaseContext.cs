// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.Provider
{
    public abstract class BaseContext
    {
        protected BaseContext(IOwinContext context)
        {
            OwinContext = context;
        }

        public IOwinContext OwinContext { get; private set; }
        public IOwinRequest Request { get { return OwinContext.Request; } }
        public IOwinResponse Response { get { return OwinContext.Response; } }
    }
}
