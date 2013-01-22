// <copyright file="Constants.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
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

namespace Microsoft.Owin.Host.SystemWeb
{
    internal static class Constants
    {
        internal const string OwinVersion = "1.0";

        internal const string ServerNameKey = "server.Name";

        internal static readonly string ServerName =
#if NET40
            "System.Web 4.0, Microsoft.Owin.Host.SystemWeb "
#endif
#if NET45
            "System.Web 4.5, Microsoft.Owin.Host.SystemWeb "
#endif
                + typeof(Constants).Assembly.GetName().Version.ToString();

        internal const string ServerVersionKey = "mssystemweb.AdapterVersion";
        internal static readonly string ServerVersion = typeof(Constants).Assembly.GetName().Version.ToString();

        internal const string ServerCapabilitiesKey = "server.Capabilities";

        internal const string SendFileVersionKey = "sendfile.Version";
        internal const string SendFileVersion = "1.0";

        internal const string SendFileFuncKey = "sendfile.Func";

        internal const string WebSocketVersionKey = "websocket.Version";
        internal const string WebSocketVersion = "1.0";
        internal const string WebSocketSubProtocolKey = "websocket.SubProtocol";

        internal const string HostOnAppDisposingKey = "host.OnAppDisposing";
        internal const string HostAppNameKey = "host.AppName";
        internal const string HostTraceOutputKey = "host.TraceOutput";
        internal const string AppModeDevelopment = "development";

        internal const string BuilderDefaultAppKey = "builder.DefaultApp";

        internal const string OwinConfiguration = "owin:Configuration";
        internal const string OwinHandleAllRequests = "owin:HandleAllRequests";
        internal const string OwinSetCurrentDirectory = "owin:SetCurrentDirectory";

        internal const string OwinResponseStatusCodeKey = "owin.ResponseStatusCode";
    }
}
