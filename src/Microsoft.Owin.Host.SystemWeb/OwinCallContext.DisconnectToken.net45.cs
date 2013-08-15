// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET50

using System;
using System.Threading;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal partial class OwinCallContext
    {
        // called when write or flush encounters HttpException
        // does nothing on NET45+ 
        private static readonly Action OnFaulted = () => { };

        internal CancellationToken BindDisconnectNotification()
        {
            return _httpResponse.ClientDisconnectedToken;
        }

        private static void UnbindDisconnectNotification()
        {
        }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
