// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Host.HttpListener
{
    internal static class Constants
    {
        internal const string VersionKey = "owin.Version";
        internal const string OwinVersion = "1.0";
        internal const string CallCancelledKey = "owin.CallCancelled";

        internal const string ServerCapabilitiesKey = "server.Capabilities";
        internal const string ServerNameKey = "server.Name";

        internal const string RequestBodyKey = "owin.RequestBody";
        internal const string RequestHeadersKey = "owin.RequestHeaders";
        internal const string RequestSchemeKey = "owin.RequestScheme";
        internal const string RequestMethodKey = "owin.RequestMethod";
        internal const string RequestPathBaseKey = "owin.RequestPathBase";
        internal const string RequestPathKey = "owin.RequestPath";
        internal const string RequestQueryStringKey = "owin.RequestQueryString";
        internal const string HttpRequestProtocolKey = "owin.RequestProtocol";
        internal const string HttpResponseProtocolKey = "owin.ResponseProtocol";

        internal const string ResponseStatusCodeKey = "owin.ResponseStatusCode";
        internal const string ResponseReasonPhraseKey = "owin.ResponseReasonPhrase";
        internal const string ResponseHeadersKey = "owin.ResponseHeaders";
        internal const string ResponseBodyKey = "owin.ResponseBody";

        internal const string ClientCertifiateKey = "ssl.ClientCertificate";
        internal const string LoadClientCertAsyncKey = "ssl.LoadClientCertAsync";

        internal const string RemoteIpAddressKey = "server.RemoteIpAddress";
        internal const string RemotePortKey = "server.RemotePort";
        internal const string LocalIpAddressKey = "server.LocalIpAddress";
        internal const string LocalPortKey = "server.LocalPort";
        internal const string IsLocalKey = "server.IsLocal";
        internal const string ServerOnSendingHeadersKey = "server.OnSendingHeaders";
        internal const string ServerUserKey = "server.User";
        internal const string ServerLoggerFactoryKey = "server.LoggerFactory";
        internal const string HostAddressesKey = "host.Addresses";

        internal const string WebSocketVersionKey = "websocket.Version";
        internal const string WebSocketVersion = "1.0";
        internal const string WebSocketAcceptKey = "websocket.Accept";
        internal const string WebSocketSubProtocolKey = "websocket.SubProtocol";

        internal const string HostHeader = "Host";
        internal const string WwwAuthenticateHeader = "WWW-Authenticate";
        internal const string ContentLengthHeader = "Content-Length";
        internal const string TransferEncodingHeader = "Transfer-Encoding";
        internal const string KeepAliveHeader = "Keep-Alive";
        internal const string ConnectionHeader = "Connection";
        internal const string SecWebSocketProtocol = "Sec-WebSocket-Protocol";

        internal const int ErrorConnectionNoLongerValid = 1229;

#if NET40
        internal const string ServerName = "HttpListener .NET 4.0, Microsoft.Owin.Host.HttpListener 2.0.0.0";
#else
        internal const string ServerName = "HttpListener .NET 4.5, Microsoft.Owin.Host.HttpListener 2.0.0.0";
#endif
    }
}
