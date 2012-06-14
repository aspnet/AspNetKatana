using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin;

[assembly: Katana.Server.HttpListenerWrapper.ServerFactory]

namespace Katana.Server.HttpListenerWrapper
{
    public class ServerFactory : Attribute
    {
        public static IDisposable Create(AppTaskDelegate app, string url)
        {
            OwinHttpListener owinListener = new OwinHttpListener();
            owinListener.Listener.Prefixes.Add(url);
            owinListener.StartProcessingRequests(app);
            return owinListener;
        }

        public static IDisposable Create(AppDelegate app, string url)
        {
            OwinHttpListener owinListener = new OwinHttpListener();
            owinListener.Listener.Prefixes.Add(url);
            owinListener.StartProcessingRequests(app);
            return owinListener;
        }
    }
}
