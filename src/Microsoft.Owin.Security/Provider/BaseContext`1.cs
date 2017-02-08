// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.Provider
{
    /// <summary>
    /// Base class used for certain event contexts
    /// </summary>
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
