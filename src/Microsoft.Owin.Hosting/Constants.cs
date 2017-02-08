// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Owin.Hosting
{
    internal static class Constants
    {
        internal const string HostTraceOutput = "host.TraceOutput";
        internal const string HostTraceSource = "host.TraceSource";
        internal const string HostOnAppDisposing = "host.OnAppDisposing";
        internal const string HostAddresses = "host.Addresses";
        internal const string HostAppName = "host.AppName";
        internal const string HostAppMode = "host.AppMode";
        internal const string AppModeDevelopment = "development";

        internal const string OwinServerFactory = "OwinServerFactory";
        internal const string SettingsOwinServer = "owin:Server";
        internal const string EnvOwnServer = "OWIN_SERVER";
        internal const string DefaultServer = "Microsoft.Owin.Host.HttpListener";
        internal const string SettingsPort = "owin:Port";
        internal const string EnvPort = "PORT";
        internal const int DefaultPort = 5000;

        internal const string SettingsOwinAppStartup = "owin:AppStartup";

        internal const string Scheme = "scheme";
        internal const string Host = "host";
        internal const string Port = "port";
        internal const string Path = "path";
    }
}
