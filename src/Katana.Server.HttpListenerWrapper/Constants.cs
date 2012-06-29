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
        public const string RemoteHostKey = "owin.RemoteHost";
        public const string HttpRequestProtocolKey = "owin.RequestProtocol";
        public const string HttpResponseProtocolKey = "owin.ResponseProtocol";
        public const string ReasonPhraseKey = "owin.ReasonPhrase"; // TODO: Is this the correct key?
        public const string ClientCertifiateKey = "owin.ClientCertificate"; // TODO: Is this the correct key?

        public const string RemoteEndPointKey = "host.RemoteEndPoint"; // TODO: Is this the correct key?  Should owin.RemoteHost have both IP and port?
        public const string LocalEndPointKey = "host.LocalEndPoint"; // TODO: Is this the correct key?
        public const string IsLocalKey = "host.IsLocal"; // TODO: Is this the correct key?
    }
}
