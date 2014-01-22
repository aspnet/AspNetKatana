// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Host.SystemWeb
{
    internal static class Constants
    {
        internal const string OwinVersion = "1.0";
        internal const string OwinVersionKey = "owin.Version";

        internal const string ServerCapabilitiesKey = "server.Capabilities";

        internal const string SendFileVersionKey = "sendfile.Version";
        internal const string SendFileVersion = "1.0";

        internal const string SendFileFuncKey = "sendfile.Func";

        internal const string WebSocketVersionKey = "websocket.Version";
        internal const string WebSocketVersion = "1.0";
        internal const string WebSocketSubProtocolKey = "websocket.SubProtocol";

        internal const string HostReferencedAssemblies = "host.ReferencedAssemblies";
        internal const string HostOnAppDisposingKey = "host.OnAppDisposing";
        internal const string HostAppNameKey = "host.AppName";
        internal const string HostTraceOutputKey = "host.TraceOutput";
        internal const string HostAppModeKey = "host.AppMode";
        internal const string AppModeDevelopment = "development";

        internal const string OwinAppStartup = "owin:AppStartup";
        internal const string OwinAutomaticAppStartup = "owin:AutomaticAppStartup";

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

        internal const string ContentType = "Content-Type";
        internal const string CacheControl = "Cache-Control";

        internal const string SecurityDataProtectionProvider = "security.DataProtectionProvider";
    }
}
