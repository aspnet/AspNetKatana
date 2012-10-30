// <copyright file="Constants.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
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

namespace Microsoft.Owin.WebSockets
{
    /// <summary>
    /// Standard keys and values for use within the OWIN interfaces
    /// </summary>
    internal static class Constants
    {
        internal const string VersionKey = "owin.Version";
        internal const string OwinVersion = "1.0";
        internal const string RequestSchemeKey = "owin.RequestScheme";
        internal const string RequestMethodKey = "owin.RequestMethod";
        internal const string RequestPathBaseKey = "owin.RequestPathBase";
        internal const string RequestPathKey = "owin.RequestPath";
        internal const string RequestQueryStringKey = "owin.RequestQueryString";
        internal const string HttpRequestProtocolKey = "owin.RequestProtocol";
        internal const string HttpResponseProtocolKey = "owin.ResponseProtocol";
        internal const string ReasonPhraseKey = "owin.ReasonPhrase";
        internal const string CallCancelledKey = "owin.CallCancelled";
        internal const string ResponseBodyKey = "owin.ResponseBody";

        internal const string ResponseHeadersKey = "owin.ResponseHeaders";
        internal const string ResponseStatusCodeKey = "owin.ResponseStatusCode";

        internal const string ServerCapabilitiesKey = "server.Capabilities";

        internal const string ClientCertifiateKey = "ssl.ClientCertificate";

        internal const string RemoteEndPointKey = "host.RemoteEndPoint";
        internal const string LocalEndPointKey = "host.LocalEndPoint";
        internal const string IsLocalKey = "host.IsLocal";

        internal const string WebSocketAcceptKey = "websocket.Accept";
        internal const string WebSocketSubProtocolKey = "websocket.SubProtocol";
        internal const string WebSocketSendAsyncKey = "websocket.SendAsync";
        internal const string WebSocketReceiveAyncKey = "websocket.ReceiveAsync";
        internal const string WebSocketCloseAsyncKey = "websocket.CloseAsync";
        internal const string WebSocketCallCancelledKey = "websocket.CallCancelled";
        internal const string WebSocketVersionKey = "websocket.Version";
        internal const string WebSocketVersion = "1.0";
        internal const string WebSocketCloseStatusKey = "websocket.ClientCloseStatus";
        internal const string WebSocketCloseDescriptionKey = "websocket.ClientCloseDescription";

        internal const string WwwAuthenticateHeader = "WWW-Authenticate";
        internal const string ContentLengthHeader = "Content-Length";
        internal const string TransferEncodingHeader = "Transfer-Encoding";
        internal const string KeepAliveHeader = "Keep-Alive";
        internal const string ConnectionHeader = "Connection";
        internal const string SecWebSocketProtocol = "Sec-WebSocket-Protocol";
    }
}
