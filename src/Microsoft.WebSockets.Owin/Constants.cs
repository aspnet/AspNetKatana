//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.WebSockets.Owin
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
        public const string CallCancelledKey = "owin.CallCancelled";

        public const string ResponseHeadersKey = "owin.ResponseHeaders";
        public const string ResponseStatusCodeKey = "owin.ResponseStatusCode";

        public const string ServerCapabilitiesKey = "server.Capabilities";

        public const string ClientCertifiateKey = "ssl.ClientCertificate";

        public const string RemoteEndPointKey = "host.RemoteEndPoint";
        public const string LocalEndPointKey = "host.LocalEndPoint";
        public const string IsLocalKey = "host.IsLocal";
        
        public const string WebSocketAcceptKey = "websocket.Accept";
        public const string WebSocketSubProtocolKey = "websocket.SubProtocol";
        public const string WebSocketSendAsyncKey = "websocket.SendAsync";
        public const string WebSocketReceiveAyncKey = "websocket.ReceiveAsync";
        public const string WebSocketCloseAsyncKey = "websocket.CloseAsync";
        public const string WebSocketCallCancelledKey = "websocket.CallCancelled";
        public const string WebSocketVersionKey = "websocket.Version";
        public const string WebSocketVersion = "1.0";
        public const string WebSocketCloseStatusKey = "websocket.ClientCloseStatus";
        public const string WebSocketCloseDescriptionKey = "websocket.ClientCloseDescription";

        public const string WwwAuthenticateHeader = "WWW-Authenticate";
        public const string ContentLengthHeader = "Content-Length";
        public const string TransferEncodingHeader = "Transfer-Encoding";
        public const string KeepAliveHeader = "Keep-Alive";
        public const string ConnectionHeader = "Connection";
        public const string SecWebSocketProtocol = "Sec-WebSocket-Protocol";
    }
}
