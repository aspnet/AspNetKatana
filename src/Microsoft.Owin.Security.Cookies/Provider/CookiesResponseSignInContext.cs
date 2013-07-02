// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;

namespace Microsoft.Owin.Security.Cookies
{
    /// <summary>
    /// Context object passed to the ICookiesAuthenticationProvider method ResponseSignIn.
    /// </summary>    
    public class CookiesResponseSignInContext
    {
        /// <summary>
        /// Creates a new instance of the context object.
        /// </summary>
        /// <param name="request">Initializes Request property</param>
        /// <param name="response">Initializes Response property</param>
        /// <param name="authenticationType">Initializes AuthenticationType property</param>
        /// <param name="identity">Initializes Identity property</param>
        /// <param name="extra">Initializes Extra property</param>
        public CookiesResponseSignInContext(
            IOwinRequest request, 
            IOwinResponse response, 
            string authenticationType,
            ClaimsIdentity identity,
            AuthenticationExtra extra)
        {
            Request = request;
            Response = response;
            AuthenticationType = authenticationType;
            Identity = identity;
            Extra = extra;
        }

        /// <summary>
        /// Used to access properties of the current request 
        /// </summary>
        public IOwinRequest Request { get; private set; }

        /// <summary>
        /// Used to affect aspects of the current response
        /// </summary>
        public IOwinResponse Response { get; private set; }

        /// <summary>
        /// The name of the AuthenticationType creating a cookie
        /// </summary>
        public string AuthenticationType { get; private set; }

        /// <summary>
        /// Contains the claims about to be converted into the outgoing cookie.
        /// May be replaced or altered during the ResponseSignIn call.
        /// </summary>
        public ClaimsIdentity Identity { get; set; }

        /// <summary>
        /// Contains the extra data about to be contained in the outgoing cookie.
        /// May be replaced or altered during the ResponseSignIn call.
        /// </summary>
        public AuthenticationExtra Extra { get; set; }
    }
}
