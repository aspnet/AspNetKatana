// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.OAuth
{
    /// <summary>
    /// Specifies callback methods which the <see cref="OAuthBearerAuthenticationMiddleware"></see> invokes to enable developer control over the authentication process. />
    /// </summary>
    public interface IOAuthBearerAuthenticationProvider
    {
        /// <summary>
        /// Invoked before the <see cref="System.Security.Claims.ClaimsIdentity"/> is created. Gives the application an 
        /// opportinity to find the identity from a different location, adjust, or reject the token.
        /// </summary>
        /// <param name="context">Contains the token string.</param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        Task RequestToken(OAuthRequestTokenContext context);

        /// <summary>
        /// Called each time a request identity has been validated by the middleware. By implementing this method the
        /// application may alter or reject the identity which has arrived with the request.
        /// </summary>
        /// <param name="context">Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        Task ValidateIdentity(OAuthValidateIdentityContext context);
    }

    /// <summary>
    /// Specifies the HTTP header for the bearer authentication scheme.
    /// </summary>
    public class OAuthRequestTokenContext : BaseContext
    {
        /// <summary>
        /// Initializes a new <see cref="OAuthRequestTokenContext"/>
        /// </summary>
        /// <param name="context">OWIN environment</param>
        /// <param name="token">The authorization header value.</param>
        public OAuthRequestTokenContext(
            IOwinContext context,
            string token) : base(context)
        {
            Token = token;
        }

        /// <summary>
        /// The authorization header value
        /// </summary>
        public string Token { get; set; }
    }
}
