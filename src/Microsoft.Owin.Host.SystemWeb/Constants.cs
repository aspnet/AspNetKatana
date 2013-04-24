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

namespace Microsoft.Owin.Host.SystemWeb
{
    internal static class Constants
    {
        internal const string OwinVersion = "1.0";
        internal const string OwinVersionKey = "owin.Version";

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

        internal const string OwinConfiguration = "owin:Configuration";
        internal const string OwinHandleAllRequests = "owin:HandleAllRequests";
        internal const string OwinSetCurrentDirectory = "owin:SetCurrentDirectory";

        internal const string OwinResponseStatusCodeKey = "owin.ResponseStatusCode";

        internal const string BuilderDefaultApp = "builder.DefaultApp";

        internal const string IntegratedPipelineContext = "integratedpipeline.Context";
        internal const string IntegratedPipelineStageMarker = "integratedpipeline.StageMarker";
        internal const string IntegratedPipelineCurrentStage = "integratedpipeline.CurrentStage";

        internal const string PersistentKey = ".persistent";
        internal const string ApplicationAuthenticationType = "Application";
        internal const string CaptionKey = "Caption";

        internal const string StageAuthenticate = "Authenticate";
        internal const string StagePostAuthenticate = "PostAuthenticate";
        internal const string StageAuthorize = "Authorize";
        internal const string StagePostAuthorize = "PostAuthorize";
        internal const string StageResolveCache = "ResolveCache";
        internal const string StagePostResolveCache = "PostResolveCache";
        internal const string StageMapHandler = "MapHandler";
        internal const string StagePostMapHandler = "PostMapHandler";
        internal const string StageAcquireState = "AcquireState";
        internal const string StagePostAcquireState = "PostAcquireState";
        internal const string StagePreHandlerExecute = "PreHandlerExecute";
    }
}
