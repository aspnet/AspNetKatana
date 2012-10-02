//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Katana.Auth.Owin
{
    /// <summary>
    /// Standard keys and values for use within the OWIN interfaces
    /// </summary>
    internal static class Constants
    {
        public const string RequestHeadersKey = "owin.RequestHeaders";
        public const string ResponseHeadersKey = "owin.ResponseHeaders";
        public const string ResponseStatusCodeKey = "owin.ResponseStatusCode";

        public const string ServerUserKey = "server.User";
        public const string ServerOnSendingHeadersKey = "server.OnSendingHeaders";

        public const string WwwAuthenticateHeader = "WWW-Authenticate";
        public const string AuthorizationHeader = "Authorization";
    }
}
