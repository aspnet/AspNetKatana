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
        /// <param name="properties">The addresses to listen on.</param>
        /// <returns>The OwinHttpListener.  Invoke Dispose to shut down.</returns>
        public static IDisposable Create(AppDelegate app, IDictionary<string, object> properties)
        {
            var servers = new List<OwinHttpListener>();
            var dispose = DisposeAll(servers);

            try
            {
                var addresses = properties.Get<IList<IDictionary<string, object>>>("host.Addresses");
                foreach (var address in addresses)
                {
                    // build url from parts
                    var scheme = address.Get<string>("scheme");
                    var host = address.Get<string>("host");
                    var port = address.Get<string>("port");
                    var path = address.Get<string>("path");

                    // if port is present, add delimiter to value before concatination
                    if (!string.IsNullOrWhiteSpace(port))
                        port = ":" + port;

                    // add a server for each url
                    var url = scheme + "://" + host + port + path + "/";
                    servers.Add(new OwinHttpListener(app, url));
                }

                // start them all
                foreach (var server in servers)
                {
                    server.Start();
                }

                // return a disposable which will stop them all
                return new Disposable(dispose);
            }
            catch
            {
                // failure to start any - dispose all and rethrow error
                try { dispose.Invoke(); }
                catch { }
                throw;
            }
        }

        private static Action DisposeAll(IEnumerable<OwinHttpListener> servers)
        {
            return () =>
            {
                var errors = new List<Exception>();
                foreach (var server in servers)
                {
                    try
                    {
                        server.Dispose();
                    }
                    catch (Exception error)
                    {
                        errors.Add(error);
                    }
                }
                if (errors.Count != 0)
                {
                    throw new AggregateException(errors);
                }
            };
        }
    }
}
