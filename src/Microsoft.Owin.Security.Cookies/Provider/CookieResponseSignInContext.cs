// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.Cookies
{
    /// <summary>
    /// Context object passed to the ICookieAuthenticationProvider method ResponseSignIn.
    /// </summary>    
    public class CookieResponseSignInContext : BaseContext<CookieAuthenticationOptions>
    {
        /// <summary>
        /// Creates a new instance of the context object.
        /// </summary>
        /// <param name="request">Initializes Request property</param>
        /// <param name="response">Initializes Response property</param>
        /// <param name="authenticationType">Initializes AuthenticationType property</param>
        /// <param name="identity">Initializes Identity property</param>
        /// <param name="properties">Initializes Extra property</param>
        [Obsolete("Replaced with a new constructor")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "response", Justification = "Obsolete")]
        public CookieResponseSignInContext(
            IOwinRequest request,
            IOwinResponse response,
            string authenticationType,
            ClaimsIdentity identity,
            AuthenticationProperties properties)
            : base((request != null ? request.Context : null), null)
        {
            AuthenticationType = authenticationType;
            Identity = identity;
            Properties = properties;
        }

        /// <summary>
        /// Creates a new instance of the context object.
        /// </summary>
        /// <param name="context">The OWIN request context</param>
        /// <param name="options">The middleware options</param>
        /// <param name="authenticationType">Initializes AuthenticationType property</param>
        /// <param name="identity">Initializes Identity property</param>
        /// <param name="properties">Initializes Extra property</param>
        public CookieResponseSignInContext(
            IOwinContext context,
            CookieAuthenticationOptions options,
            string authenticationType,
            ClaimsIdentity identity,
            AuthenticationProperties properties)
            : base(context, options)
        {
            AuthenticationType = authenticationType;
            Identity = identity;
            Properties = properties;
        }

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
        public AuthenticationProperties Properties { get; set; }
    }
}
