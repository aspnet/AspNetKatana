// <copyright file="xxx" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if !NET40

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.Host.SystemWeb.WebSockets;

namespace Microsoft.Owin.Host.SystemWeb
{
    using WebSocketAccept =
        Action<IDictionary<string, object>, // WebSocket Accept parameters
            Func<IDictionary<string, object>, // WebSocket environment
                Task /* Complete */>>;
    using WebSocketFunc =
        Func<IDictionary<string, object>, // WebSocket environment
            Task /* Complete */>;

    internal partial class OwinCallContext
    {
        private WebSocketFunc _webSocketFunc;
        private IDictionary<string, object> _acceptOptions;

        private WebSocketAccept BindWebSocketAccept()
        {
            if (WebSocketHelpers.IsAspNetWebSocketRequest(_httpContext))
            {
                return new WebSocketAccept(
                    (options, callback) =>
                    {
                        _env.ResponseStatusCode = 101;
                        _acceptOptions = options;
                        _webSocketFunc = callback;
                    });
            }
            return null;
        }
    }
}

#endif
