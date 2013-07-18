// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.Provider
{
    public abstract class BaseContext<TOptions>
    {
        protected BaseContext(IOwinContext context, TOptions options)
        {
            OwinContext = context;
            Options = options;
        }

        public IOwinContext OwinContext { get; private set; }

        public TOptions Options { get; private set; }

        public IOwinRequest Request
        {
            get { return OwinContext.Request; }
        }

        public IOwinResponse Response
        {
            get { return OwinContext.Response; }
        }
    }
}
