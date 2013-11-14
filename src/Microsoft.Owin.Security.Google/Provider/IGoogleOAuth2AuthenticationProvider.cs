// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Google
{
    /// <summary>
    /// Specifies callback methods which the <see cref="GoogleOAuth2AuthenticationMiddleware"></see> invokes to enable developer control over the authentication process. />
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Auth",
        Justification = "OAuth2 is a valid word.")]
    public interface IGoogleOAuth2AuthenticationProvider
    {
        /// <summary>
        /// Invoked whenever Google succesfully authenticates a user
        /// </summary>
        /// <param name="context">Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        Task Authenticated(GoogleOAuth2AuthenticatedContext context);

        /// <summary>
        /// Invoked prior to the <see cref="System.Security.Claims.ClaimsIdentity"/> being saved in a local cookie and the browser being redirected to the originally requested URL.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        Task ReturnEndpoint(GoogleOAuth2ReturnEndpointContext context);
    }
}
