//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Katana.Auth.Owin
{
    internal static class Constants
    {
        internal const string RequestSchemeKey = "owin.RequestScheme";
        internal const string RequestHeadersKey = "owin.RequestHeaders";
        internal const string ResponseHeadersKey = "owin.ResponseHeaders";
        internal const string ResponseStatusCodeKey = "owin.ResponseStatusCode";
        internal const string ResponseReasonPhraseKey = "owin.ResponseReasonPhrase";

        internal const string ServerUserKey = "server.User";
        internal const string ServerOnSendingHeadersKey = "server.OnSendingHeaders";

        internal const string WwwAuthenticateHeader = "WWW-Authenticate";
        internal const string AuthorizationHeader = "Authorization";
    }
}
