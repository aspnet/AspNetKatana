// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.Cookies
{
    /// <summary>
    /// Context object passed to the ICookieAuthenticationProvider method ResponseSignOut    
    /// </summary>
    public class CookieResponseSignOutContext : BaseContext<CookieAuthenticationOptions>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="cookieOptions"></param>
        public CookieResponseSignOutContext(IOwinContext context, CookieAuthenticationOptions options, CookieOptions cookieOptions)
            : base(context, options)
        {
            CookieOptions = cookieOptions;
        }

        /// <summary>
        /// The options for creating the outgoing cookie.
        /// May be replace or altered during the ResponseSignOut call.
        /// </summary>
        public CookieOptions CookieOptions
        {
            get;
            set;
        }
    }
}
