[assembly: Katana.Server.HttpListenerWrapper.ServerFactory]

namespace Katana.Server.HttpListenerWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Owin;

    /// <summary>
    /// Implements the Katana setup pattern for the OwinHttpListener server.
    /// </summary>
    public class ServerFactory : Attribute
    {
        /*
        public static IDisposable Create(AppTaskDelegate app, string url)
        {
            OwinHttpListener owinListener = new OwinHttpListener();
            owinListener.Listener.Prefixes.Add(url);
            owinListener.StartProcessingRequests(app);
            return owinListener;
        }
        */
        public static IDisposable Create(AppDelegate app, string url)
        {
            OwinHttpListener owinListener = new OwinHttpListener(url);
            owinListener.StartProcessingRequests(app);
            return owinListener;
        }
    }
}
