//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

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
        /// <summary>
        /// Creates an OwinHttpListener and starts listening on the given url.
        /// </summary>
        /// <param name="app">The application entry point.</param>
        /// <param name="url">The url to listen on.</param>
        /// <returns>The OwinHttpListener.  Invoke Dispose to shut down.</returns>
        public static IDisposable Create(AppDelegate app, string url)
        {
            OwinHttpListener owinListener = new OwinHttpListener(app, url);
            owinListener.Start();
            return owinListener;
        }
    }
}
