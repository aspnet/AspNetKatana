//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.HttpListener.Owin
{
    /// <summary>
    /// Standard keys and values for use within the OWIN interfaces
    /// </summary>
    internal static class Constants
    {
        public const string VersionKey = "owin.Version";
        public const string OwinVersion = "1.0";
        public const string CallCancelledKey = "owin.CallCancelled";

        public const string ServerCapabilitiesKey = "server.Capabilities";
        public const string ServerNameKey = "server.Name";
        public static readonly string ServerName = "HttpListener .NET 4.0, Microsoft.HttpListener.Owin " + typeof(Constants).Assembly.GetName().Version.ToString();
        public const string ServerVersionKey = "mshttplistener.AdapterVersion";
        public static readonly string ServerVersion = typeof(Constants).Assembly.GetName().Version.ToString();

        public const string RequestBodyKey = "owin.RequestBody";
        public const string RequestHeadersKey = "owin.RequestHeaders";
        public const string RequestSchemeKey = "owin.RequestScheme";
        public const string RequestMethodKey = "owin.RequestMethod";
        public const string RequestPathBaseKey = "owin.RequestPathBase";
        public const string RequestPathKey = "owin.RequestPath";
        public const string RequestQueryStringKey = "owin.RequestQueryString";
        public const string HttpRequestProtocolKey = "owin.RequestProtocol";
        public const string HttpResponseProtocolKey = "owin.ResponseProtocol";

        public const string ResponseStatusCodeKey = "owin.ResponseStatusCode";
        public const string ResponseReasonPhraseKey = "owin.ResponseReasonPhrase";
        public const string ResponseHeadersKey = "owin.ResponseHeaders";
        public const string ResponseBodyKey = "owin.ResponseBody";

        public const string ClientCertifiateKey = "ssl.ClientCertificate";

        public const string RemoteIpAddressKey = "server.RemoteIpAddress";
        public const string RemotePortKey = "server.RemotePort";
        public const string LocalIpAddressKey = "server.LocalIpAddress";
        public const string LocalPortKey = "server.LocalPort";
        public const string IsLocalKey = "server.IsLocal";
        public const string ServerOnSendingHeadersKey = "server.OnSendingHeaders";

        public const string WebSocketSupportKey = "websocket.Support";
        public const string WebSocketFuncKey = "websocket.Accept";

        public const string WwwAuthenticateHeader = "WWW-Authenticate";
        public const string ContentLengthHeader = "Content-Length";
        public const string TransferEncodingHeader = "Transfer-Encoding";
        public const string KeepAliveHeader = "Keep-Alive";
        public const string ConnectionHeader = "Connection";
    }
}
