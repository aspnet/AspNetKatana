// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using Microsoft.Owin.Hosting.Builder;
using Microsoft.Owin.Hosting.Loader;
using Microsoft.Owin.Hosting.ServerFactory;
using Microsoft.Owin.Hosting.Tracing;
using Microsoft.Owin.Hosting.Utilities;
using Microsoft.Owin.Logging;

namespace Microsoft.Owin.Hosting.Engine
{
    /// <summary>
    /// Used to initialize and start a web application.
    /// </summary>
    public class HostingEngine : IHostingEngine
    {
        private readonly IAppBuilderFactory _appBuilderFactory;
        private readonly ITraceOutputFactory _traceOutputFactory;
        private readonly IAppLoader _appLoader;
        private readonly IServerFactoryLoader _serverFactoryLoader;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appBuilderFactory"></param>
        /// <param name="traceOutputFactory"></param>
        /// <param name="appLoader"></param>
        /// <param name="serverFactoryLoader"></param>
        /// <param name="loggerFactory"></param>
        public HostingEngine(
            IAppBuilderFactory appBuilderFactory,
            ITraceOutputFactory traceOutputFactory,
            IAppLoader appLoader,
            IServerFactoryLoader serverFactoryLoader,
            ILoggerFactory loggerFactory)
        {
            if (appBuilderFactory == null)
            {
                throw new ArgumentNullException("appBuilderFactory");
            }
            if (traceOutputFactory == null)
            {
                throw new ArgumentNullException("traceOutputFactory");
            }
            if (appLoader == null)
            {
                throw new ArgumentNullException("appLoader");
            }
            if (loggerFactory == null)
            {
                throw new ArgumentNullException("loggerFactory");
            }

            _appBuilderFactory = appBuilderFactory;
            _traceOutputFactory = traceOutputFactory;
            _appLoader = appLoader;
            _serverFactoryLoader = serverFactoryLoader;
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Initialize and start a web application.
        /// Major Steps:
        /// - Find and initialize the ServerFactory
        /// - Find and initialize the application
        /// - Start the server
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IDisposable Start(StartContext context)
        {
            ResolveOutput(context);
            InitializeBuilder(context);
            EnableTracing(context);
            IDisposable disposablePipeline = EnableDisposing(context);
            ResolveServerFactory(context);
            InitializeServerFactory(context);
            ResolveApp(context);
            IDisposable disposableServer = StartServer(context);

            return new Disposable(
                () =>
                {
                    try
                    {
                        // first stop processing requests
                        disposableServer.Dispose();
                    }
                    finally
                    {
                        // then inform the pipeline of app shutdown
                        disposablePipeline.Dispose();
                        // Flush logs
                        context.TraceOutput.Flush();
                    }
                });
        }

        /// <summary>
        /// Tries to determine a custom port setting from the startup options or the port environment variable.
        /// </summary>
        /// <param name="options">The OWIN application startup options.</param>
        /// <param name="port">The port number.</param>
        /// <returns>True if a valid custom port was set, false if not.</returns>
        public static bool TryDetermineCustomPort(StartOptions options, out int port)
        {
            string portString;
            if (options != null)
            {
                if (options.Port.HasValue)
                {
                    port = options.Port.Value;
                    return true;
                }

                IDictionary<string, string> settings = options.Settings;
                if (settings == null || !settings.TryGetValue(Constants.SettingsPort, out portString))
                {
                    portString = GetPortEnvironmentVariable();
                }
            }
            else
            {
                portString = GetPortEnvironmentVariable();
            }

            return int.TryParse(portString, NumberStyles.Integer, CultureInfo.InvariantCulture, out port);
        }

        /// <summary>
        /// Gets the default port number.
        /// </summary>
        /// <returns>The default port number.</returns>
        public static int GetDefaultPort()
        {
            return Constants.DefaultPort;
        }

        private void ResolveOutput(StartContext context)
        {
            if (context.TraceOutput == null)
            {
                string traceoutput;
                context.Options.Settings.TryGetValue("traceoutput", out traceoutput);
                context.TraceOutput = _traceOutputFactory.Create(traceoutput);
            }

            context.EnvironmentData.Add(new KeyValuePair<string, object>(Constants.HostTraceOutput, context.TraceOutput));
        }

        private void InitializeBuilder(StartContext context)
        {
            if (context.Builder == null)
            {
                context.Builder = _appBuilderFactory.Create();
            }

            var addresses = new List<IDictionary<string, object>>();

            foreach (var url in context.Options.Urls)
            {
                string scheme;
                string host;
                string port;
                string path;
                if (DeconstructUrl(url, out scheme, out host, out port, out path))
                {
                    addresses.Add(new Dictionary<string, object>
                    {
                        { Constants.Scheme, scheme },
                        { Constants.Host, host },
                        { Constants.Port, port.ToString(CultureInfo.InvariantCulture) },
                        { Constants.Path, path },
                    });
                }
            }

            if (addresses.Count == 0)
            {
                int port = DeterminePort(context);
                addresses.Add(new Dictionary<string, object>
                {
                    { Constants.Port, port.ToString(CultureInfo.InvariantCulture) },
                });
            }

            context.Builder.Properties[Constants.HostAddresses] = addresses;

            if (!string.IsNullOrWhiteSpace(context.Options.AppStartup))
            {
                context.Builder.Properties[Constants.HostAppName] = context.Options.AppStartup;
                context.EnvironmentData.Add(new KeyValuePair<string, object>(Constants.HostAppName, context.Options.AppStartup));
            }

            // This key lets us know the app was launched from Visual Studio.
            string vsVersion = Environment.GetEnvironmentVariable("VisualStudioVersion");
            if (!string.IsNullOrWhiteSpace(vsVersion))
            {
                context.Builder.Properties[Constants.HostAppMode] = Constants.AppModeDevelopment;
                context.EnvironmentData.Add(new KeyValuePair<string, object>(Constants.HostAppMode, Constants.AppModeDevelopment));
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#",
            Justification = "The host may contain wildcards not supported by System.Uri")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "By design")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "By design")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#", Justification = "By design")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "4#", Justification = "By design")]
        internal static bool DeconstructUrl(
            string url,
            out string scheme,
            out string host,
            out string port,
            out string path)
        {
            url = url ?? string.Empty;

            int delimiterStart1 = url.IndexOf(Uri.SchemeDelimiter, StringComparison.Ordinal);
            if (delimiterStart1 < 0)
            {
                scheme = null;
                host = null;
                port = null;
                path = null;
                return false;
            }
            int delimiterEnd1 = delimiterStart1 + Uri.SchemeDelimiter.Length;

            int delimiterStart3 = url.IndexOf("/", delimiterEnd1, StringComparison.Ordinal);
            if (delimiterStart3 < 0)
            {
                delimiterStart3 = url.Length;
            }
            int delimiterStart2 = url.LastIndexOf(":", delimiterStart3 - 1, delimiterStart3 - delimiterEnd1, StringComparison.Ordinal);
            int delimiterEnd2 = delimiterStart2 + ":".Length;
            if (delimiterStart2 < 0)
            {
                delimiterStart2 = delimiterStart3;
                delimiterEnd2 = delimiterStart3;
            }

            scheme = url.Substring(0, delimiterStart1);
            string portString = url.Substring(delimiterEnd2, delimiterStart3 - delimiterEnd2);
            int ignored;
            if (int.TryParse(portString, NumberStyles.Integer, CultureInfo.InvariantCulture, out ignored))
            {
                host = url.Substring(delimiterEnd1, delimiterStart2 - delimiterEnd1);
                port = portString;
            }
            else
            {
                if (string.Equals(scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
                {
                    port = "80";
                }
                else if (string.Equals(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                {
                    port = "443";
                }
                else
                {
                    port = string.Empty;
                }
                host = url.Substring(delimiterEnd1, delimiterStart3 - delimiterEnd1);
            }
            path = url.Substring(delimiterStart3);
            return true;
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Partial trust not supported")]
        private void EnableTracing(StartContext context)
        {
            // string etwGuid = "CB50EAF9-025E-4CFB-A918-ED0F7C0CD0FA";
            // EventProviderTraceListener etwListener = new EventProviderTraceListener(etwGuid, "HostingEtwListener", "::");
            var textListener = new TextWriterTraceListener(context.TraceOutput, "HostingTraceListener");

            Trace.Listeners.Add(textListener);
            // Trace.Listeners.Add(etwListener);

            var source = new TraceSource("HostingTraceSource", SourceLevels.All);
            source.Listeners.Add(textListener);
            // source.Listeners.Add(etwListener);

            context.Builder.Properties[Constants.HostTraceOutput] = context.TraceOutput;
            context.Builder.Properties[Constants.HostTraceSource] = source;

            context.Builder.SetLoggerFactory(_loggerFactory);
        }

        private static IDisposable EnableDisposing(StartContext context)
        {
            var cts = new CancellationTokenSource();
            context.Builder.Properties[Constants.HostOnAppDisposing] = cts.Token;
            context.EnvironmentData.Add(new KeyValuePair<string, object>(Constants.HostOnAppDisposing, cts.Token));
            return new Disposable(() => cts.Cancel(false));
        }

        private void ResolveServerFactory(StartContext context)
        {
            if (context.ServerFactory != null)
            {
                return;
            }

            string serverName = DetermineOwinServer(context);
            context.ServerFactory = _serverFactoryLoader.Load(serverName);
            if (context.ServerFactory == null)
            {
                throw new MissingMemberException(string.Format(CultureInfo.InvariantCulture, Resources.Exception_ServerNotFound, serverName));
            }
        }

        private static string DetermineOwinServer(StartContext context)
        {
            StartOptions options = context.Options;
            IDictionary<string, string> settings = context.Options.Settings;

            string serverName = options.ServerFactory;
            if (!string.IsNullOrWhiteSpace(serverName))
            {
                return serverName;
            }

            if (settings != null &&
                settings.TryGetValue(Constants.SettingsOwinServer, out serverName) &&
                !string.IsNullOrWhiteSpace(serverName))
            {
                return serverName;
            }

            serverName = Environment.GetEnvironmentVariable(Constants.EnvOwnServer, EnvironmentVariableTarget.Process);
            if (!string.IsNullOrWhiteSpace(serverName))
            {
                return serverName;
            }

            return Constants.DefaultServer;
        }

        private static int DeterminePort(StartContext context)
        {
            int port;
            if (!TryDetermineCustomPort(context.Options, out port))
            {
                port = GetDefaultPort();
            }

            return port;
        }

        private static string GetPortEnvironmentVariable()
        {
            return Environment.GetEnvironmentVariable(Constants.EnvPort, EnvironmentVariableTarget.Process);
        }

        private static string DetermineApplicationName(StartContext context)
        {
            StartOptions options = context.Options;
            IDictionary<string, string> settings = context.Options.Settings;

            if (options != null && !string.IsNullOrWhiteSpace(options.AppStartup))
            {
                return options.AppStartup;
            }

            string appName;
            if (settings.TryGetValue(Constants.SettingsOwinAppStartup, out appName) &&
                !string.IsNullOrWhiteSpace(appName))
            {
                return appName;
            }

            return null;
        }

        private static void InitializeServerFactory(StartContext context)
        {
            context.ServerFactory.Initialize(context.Builder);
        }

        private void ResolveApp(StartContext context)
        {
            context.Builder.Use(typeof(Encapsulate), context.EnvironmentData);

            if (context.App == null)
            {
                IList<string> errors = new List<string>();
                if (context.Startup == null)
                {
                    string appName = DetermineApplicationName(context);
                    context.Startup = _appLoader.Load(appName, errors);
                }
                if (context.Startup == null)
                {
                    throw new EntryPointNotFoundException(Resources.Exception_AppLoadFailure
                        + Environment.NewLine + " - " + string.Join(Environment.NewLine + " - ", errors));
                }
                context.Startup(context.Builder);
            }
            else
            {
                context.Builder.Use(new Func<object, object>(_ => context.App));
            }
        }

        private static IDisposable StartServer(StartContext context)
        {
            return context.ServerFactory.Create(context.Builder);
        }
    }
}
