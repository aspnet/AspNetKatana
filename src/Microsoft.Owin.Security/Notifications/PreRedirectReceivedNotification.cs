using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Notifications
{    public class PreRedirectReceivedNotification<TMessage, TOptions> : BaseNotification<TOptions>
    {
        public PreRedirectReceivedNotification(IOwinContext context, TOptions options)
            : base(context, options)
        {
        }

        /// <summary>
        /// Gets or set the <see cref="AuthenticationTicket"/>
        /// </summary>
        public AuthenticationTicket AuthenticationTicket { get; set; }

        /// <summary>
        /// Redirection is handled by the calling application
        /// </summary>
        public void HandledRedirect()
        {                        
            AuthenticationTicket.Properties.Dictionary["HandledRedirect"] = "true";
        }
    }
}
