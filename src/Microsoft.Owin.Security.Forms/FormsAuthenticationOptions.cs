// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Owin.Infrastructure;

namespace Microsoft.Owin.Security.Forms
{
    /// <summary>
    /// Contains the options used by the FormsAuthenticationMiddleware
    /// </summary>
    public class FormsAuthenticationOptions : AuthenticationOptions
    {
        private string _cookieName;

        /// <summary>
        /// Create an instance of the options initialized with the default values
        /// </summary>
        public FormsAuthenticationOptions()
            : base(FormsAuthenticationDefaults.AuthenticationType)
        {
            CookieName = FormsAuthenticationDefaults.CookiePrefix + FormsAuthenticationDefaults.AuthenticationType;
            CookiePath = "/";
            ExpireTimeSpan = TimeSpan.FromDays(14);
            SlidingExpiration = true;
            CookieHttpOnly = true;
            CookieSecure = CookieSecureOption.SameAsRequest;
            SystemClock = new SystemClock();
            Provider = new FormsAuthenticationProvider();
        }

        /// <summary>
        /// Determines the cookie name used to persist the identity. The default value is ".AspNet.Forms".
        /// This value should be changed if you change the name of the AuthenticationType, especially if your
        /// system uses the cookie authentication middleware multiple times.
        /// </summary>
        public string CookieName
        {
            get { return _cookieName; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("CookieName");
                }
                _cookieName = value;
            }
        }

        /// <summary>
        /// Determines the domain used to create the cookie. Is not provided by default.
        /// </summary>
        public string CookieDomain { get; set; }

        /// <summary>
        /// Determines the path used to create the cookie. The default value is "/" for highest browser compatability.
        /// </summary>
        public string CookiePath { get; set; }

        /// <summary>
        /// Determines if the browser should allow the cookie to be accessed by client-side javascript. The
        /// default is true, which means the cookie will only be passed to http requests and is not made available
        /// to script on the page.
        /// </summary>
        public bool CookieHttpOnly { get; set; }

        /// <summary>
        /// Determines if the cookie should only be transmitted on HTTPS request. The default is to limit the cookie
        /// to HTTPS requests if the page which is doing the SignIn is also HTTPS. If you have an HTTPS sign in page
        /// and portions of your site are HTTP you may need to change this value.
        /// </summary>
        public CookieSecureOption CookieSecure { get; set; }

        /// <summary>
        /// Controls how much time the cookie will remain valid from the point it is created. The expiration
        /// information is in the protected cookie ticket. Because of that an expired cookie will be ignored 
        /// even if it is passed to the server after the browser should have purged it 
        /// </summary>
        public TimeSpan ExpireTimeSpan { get; set; }

        /// <summary>
        /// The SlidingExpiration is set to true to instruct the middleware to re-issue a new cookie with a new
        /// expiration time any time it processes a request which is more than halfway through the expiration window.
        /// </summary>
        public bool SlidingExpiration { get; set; }

        /// <summary>
        /// The LoginPath property informs the middleware that it should change an outgoing 401 Unauthorized status
        /// code into a 302 redirection onto the given login path. The current url which generated the 401 is added
        /// to the LoginPath as a query string parameter named by the ReturnUrlParameter. Once a request to the
        /// LoginPath grants a new SignIn identity, the ReturnUrlParameter value is used to redirect back to the 
        /// url 
        /// </summary>
        public string LoginPath { get; set; }
        public string LogoutPath { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "This is the name of a querystring parameter")]
        public string ReturnUrlParameter { get; set; }

        public IFormsAuthenticationProvider Provider { get; set; }

        public ISecureDataHandler<AuthenticationTicket> TicketDataHandler { get; set; }

        public ISystemClock SystemClock { get; set; }
    }
}
