using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Owin.WebSockets
{
    public static class WebSocketExtensions
    {
        public static IAppBuilder UseWebSockets(this IAppBuilder builder)
        {
            // TODO: Verify Opaque support
            // TODO: Add capability to dictionary
            return builder.UseType<OpaqueToWebSocket>();
        }
    }
}
