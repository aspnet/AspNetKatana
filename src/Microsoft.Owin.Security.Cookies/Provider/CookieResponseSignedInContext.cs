// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.Cookies
{
    /// <summary>
    /// Context object passed to the ICookieAuthenticationProvider method ResponseSignedIn.
    /// </summary>    
    public class CookieResponseSignedInContext : BaseContext<CookieAuthenticationOptions>
    {
        /// <summary>
        /// Creates a new instance of the context object.
        /// </summary>
        /// <param name="context">The OWIN request context</param>
        /// <param name="options">The middleware options</param>
        /// <param name="authenticationType">Initializes AuthenticationType property</param>
        /// <param name="identity">Initializes Identity property</param>
        /// <param name="properties">Initializes Properties property</param>
        public CookieResponseSignedInContext(
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
        /// Contains the claims that were converted into the outgoing cookie.
        /// </summary>
        public ClaimsIdentity Identity { get; private set; }

        /// <summary>
        /// Contains the extra data that was contained in the outgoing cookie.
        /// </summary>
        public AuthenticationProperties Properties { get; private set; }
    }
}
