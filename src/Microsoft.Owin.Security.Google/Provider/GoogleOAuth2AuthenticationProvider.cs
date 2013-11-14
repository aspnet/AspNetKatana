// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Google
{
        /// <summary>
    /// Default <see cref="IGoogleOAuth2AuthenticationProvider"/> implementation.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Auth",
        Justification = "OAuth2 is a valid word.")]
    public class GoogleOAuth2AuthenticationProvider : IGoogleOAuth2AuthenticationProvider
    {
        /// <summary>
        /// Initializes a <see cref="GoogleOAuth2AuthenticationProvider"/>
        /// </summary>
        public GoogleOAuth2AuthenticationProvider()
        {
            OnAuthenticated = context => Task.FromResult<object>(null);
            OnReturnEndpoint = context => Task.FromResult<object>(null);
        }

        /// <summary>
        /// Gets or sets the function that is invoked when the Authenticated method is invoked.
        /// </summary>
        public Func<GoogleOAuth2AuthenticatedContext, Task> OnAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets the function that is invoked when the ReturnEndpoint method is invoked.
        /// </summary>
        public Func<GoogleOAuth2ReturnEndpointContext, Task> OnReturnEndpoint { get; set; }

        /// <summary>
        /// Invoked whenever Google succesfully authenticates a user
        /// </summary>
        /// <param name="context">Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        public virtual Task Authenticated(GoogleOAuth2AuthenticatedContext context)
        {
            return OnAuthenticated(context);
        }

        /// <summary>
        /// Invoked prior to the <see cref="System.Security.Claims.ClaimsIdentity"/> being saved in a local cookie and the browser being redirected to the originally requested URL.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        public virtual Task ReturnEndpoint(GoogleOAuth2ReturnEndpointContext context)
        {
            return OnReturnEndpoint(context);
        }
    }
}
