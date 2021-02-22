// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.OpenIdConnect
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Auth",
        Justification = "OAuth2 is a valid word.")]
    internal static class OAuthConstants
    {
        /// <summary>
        /// code_verifier defined in https://tools.ietf.org/html/rfc7636
        /// </summary>
        public const string CodeVerifierKey = "code_verifier";

        /// <summary>
        /// code_challenge defined in https://tools.ietf.org/html/rfc7636
        /// </summary>
        public const string CodeChallengeKey = "code_challenge";

        /// <summary>
        /// code_challenge_method defined in https://tools.ietf.org/html/rfc7636
        /// </summary>
        public const string CodeChallengeMethodKey = "code_challenge_method";

        /// <summary>
        /// S256 defined in https://tools.ietf.org/html/rfc7636
        /// </summary>
        public const string CodeChallengeMethodS256 = "S256";
    }
}