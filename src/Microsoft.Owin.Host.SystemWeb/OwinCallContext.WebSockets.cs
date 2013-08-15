// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET40

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;

namespace Microsoft.Owin.Host.SystemWeb
{
    using WebSocketAccept =
        Action<IDictionary<string, object>, // WebSocket Accept parameters
            Func<IDictionary<string, object>, // WebSocket environment
                Task /* Complete */>>;

    internal partial class OwinCallContext
    {
        bool AspNetDictionary.IPropertySource.TryGetWebSocketAccept(ref WebSocketAccept value)
        {
            return false;
        }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
