// <copyright file="Constants.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
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
// </copyright>

namespace Microsoft.Owin.Hosting
{
    internal static class Constants
    {
        internal const string HostTraceOutput = "host.TraceOutput";
        internal const string HostTraceSource = "host.TraceSource";
        internal const string HostOnAppDisposing = "host.OnAppDisposing";
        internal const string HostAddresses = "host.Addresses";
        internal const string HostAppName = "host.AppName";

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
