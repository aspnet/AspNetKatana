//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Katana.Server.HttpListenerWrapper
{
    /// <summary>
    /// Standard keys and values for use within the OWIN interfaces
    /// </summary>
    internal static class Constants
    {
        public const string VersionKey = "owin.Version";
        public const string OwinVersion = "1.0";
        public const string RequestSchemeKey = "owin.RequestScheme";
        public const string RequestMethodKey = "owin.RequestMethod";
        public const string RequestPathBaseKey = "owin.RequestPathBase";
        public const string RequestPathKey = "owin.RequestPath";
        public const string RequestQueryStringKey = "owin.RequestQueryString";
        public const string HttpRequestProtocolKey = "owin.RequestProtocol";
        public const string HttpResponseProtocolKey = "owin.ResponseProtocol";
        public const string ReasonPhraseKey = "owin.ReasonPhrase";
        public const string CallCompletedKey = "owin.CallCompleted";

        public const string ClientCertifiateKey = "ssl.ClientCertificate";

        public const string RemoteEndPointKey = "host.RemoteEndPoint";
        public const string LocalEndPointKey = "host.LocalEndPoint";
        public const string IsLocalKey = "host.IsLocal";

        public const string WebSocketSupportKey = "websocket.Support";
        public const string WebSocketFuncKey = "websocket.Func";
        public const string WebSocketSupport = "WebSocketFunc";

        public const string WwwAuthenticateHeader = "WWW-Authenticate";
        public const string ContentLengthHeader = "Content-Length";
        public const string TransferEncodingHeader = "Transfer-Encoding";
        public const string KeepAliveHeader = "Keep-Alive";
        public const string ConnectionHeader = "Connection";
    }
}
