// Copyright 2011-2012 Katana contributors
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
namespace Microsoft.HttpListener.Owin
{
    internal static class Constants
    {
        internal const string VersionKey = "owin.Version";
        internal const string OwinVersion = "1.0";
        internal const string CallCancelledKey = "owin.CallCancelled";

        internal const string ServerCapabilitiesKey = "server.Capabilities";
        internal const string ServerNameKey = "server.Name";
        internal const string ServerVersionKey = "mshttplistener.AdapterVersion";

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

        internal const string RemoteIpAddressKey = "server.RemoteIpAddress";
        internal const string RemotePortKey = "server.RemotePort";
        internal const string LocalIpAddressKey = "server.LocalIpAddress";
        internal const string LocalPortKey = "server.LocalPort";
        internal const string IsLocalKey = "server.IsLocal";
        internal const string ServerOnSendingHeadersKey = "server.OnSendingHeaders";

        internal const string WebSocketSupportKey = "websocket.Support";
        internal const string WebSocketFuncKey = "websocket.Accept";

        internal const string WwwAuthenticateHeader = "WWW-Authenticate";
        internal const string ContentLengthHeader = "Content-Length";
        internal const string TransferEncodingHeader = "Transfer-Encoding";
        internal const string KeepAliveHeader = "Keep-Alive";
        internal const string ConnectionHeader = "Connection";

        internal static readonly string ServerName = "HttpListener .NET 4.0, Microsoft.HttpListener.Owin " + typeof(Constants).Assembly.GetName().Version.ToString();
        internal static readonly string ServerVersion = typeof(Constants).Assembly.GetName().Version.ToString();
    }
}
