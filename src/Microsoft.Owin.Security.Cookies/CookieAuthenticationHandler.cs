// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Infrastructure;

namespace Microsoft.Owin.Security.Cookies
{
    internal class CookieAuthenticationHandler : AuthenticationHandler<CookieAuthenticationOptions>
    {
        private const string HeaderNameCacheControl = "Cache-Control";
        private const string HeaderNamePragma = "Pragma";
        private const string HeaderNameExpires = "Expires";
        private const string HeaderValueNoCache = "no-cache";
        private const string HeaderValueMinusOne = "-1";

        private readonly ILogger _logger;

        private bool _shouldRenew;
        private DateTimeOffset _renewIssuedUtc;
        private DateTimeOffset _renewExpiresUtc;

        public CookieAuthenticationHandler(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            _logger = logger;
        }

        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            RequestCookieCollection cookies = Request.Cookies;
            string cookie = cookies[Options.CookieName];
            if (string.IsNullOrWhiteSpace(cookie))
            {
                return null;
            }

            AuthenticationTicket ticket = Options.TicketDataFormat.Unprotect(cookie);

            if (ticket == null)
            {
                _logger.WriteWarning(@"Unprotect ticket failed");
                return null;
            }

            DateTimeOffset currentUtc = Options.SystemClock.UtcNow;
            DateTimeOffset? issuedUtc = ticket.Properties.IssuedUtc;
            DateTimeOffset? expiresUtc = ticket.Properties.ExpiresUtc;

            if (expiresUtc != null && expiresUtc.Value < currentUtc)
            {
                return null;
            }

            if (issuedUtc != null && expiresUtc != null && Options.SlidingExpiration)
            {
                TimeSpan timeElapsed = currentUtc.Subtract(issuedUtc.Value);
                TimeSpan timeRemaining = expiresUtc.Value.Subtract(currentUtc);

                if (timeRemaining < timeElapsed)
                {
                    _shouldRenew = true;
                    _renewIssuedUtc = currentUtc;
                    TimeSpan timeSpan = expiresUtc.Value.Subtract(issuedUtc.Value);
                    _renewExpiresUtc = currentUtc.Add(timeSpan);
                }
            }

            var context = new CookieValidateIdentityContext(ticket);

            await Options.Provider.ValidateIdentity(context);

            return new AuthenticationTicket(context.Identity, context.Properties);
        }

        protected override async Task ApplyResponseGrantAsync()
        {
            AuthenticationResponseGrant signin = Helper.LookupSignIn(Options.AuthenticationType);
            bool shouldSignin = signin != null;
            AuthenticationResponseRevoke signout = Helper.LookupSignOut(Options.AuthenticationType, Options.AuthenticationMode);
            bool shouldSignout = signout != null;

            if (shouldSignin || shouldSignout || _shouldRenew)
            {
                var cookieOptions = new CookieOptions
                {
                    Domain = Options.CookieDomain,
                    HttpOnly = Options.CookieHttpOnly,
                    Path = Options.CookiePath ?? "/",
                };
                if (Options.CookieSecure == CookieSecureOption.SameAsRequest)
                {
                    cookieOptions.Secure = Request.IsSecure;
                }
                else
                {
                    cookieOptions.Secure = Options.CookieSecure == CookieSecureOption.Always;
                }

                if (shouldSignin)
                {
                    var context = new CookieResponseSignInContext(
                        Request,
                        Response,
                        Options.AuthenticationType,
                        signin.Identity,
                        signin.Properties);

                    DateTimeOffset issuedUtc = Options.SystemClock.UtcNow;
                    DateTimeOffset expiresUtc = issuedUtc.Add(Options.ExpireTimeSpan);

                    context.Properties.IssuedUtc = issuedUtc;
                    context.Properties.ExpiresUtc = expiresUtc;

                    Options.Provider.ResponseSignIn(context);

                    if (context.Properties.IsPersistent)
                    {
                        cookieOptions.Expires = expiresUtc.ToUniversalTime().DateTime;
                    }

                    var model = new AuthenticationTicket(context.Identity, context.Properties);
                    string cookieValue = Options.TicketDataFormat.Protect(model);

                    Response.Cookies.Append(
                        Options.CookieName,
                        cookieValue,
                        cookieOptions);
                }
                else if (shouldSignout)
                {
                    Response.Cookies.Delete(
                        Options.CookieName,
                        cookieOptions);
                }
                else if (_shouldRenew)
                {
                    AuthenticationTicket model = await AuthenticateAsync();

                    model.Properties.IssuedUtc = _renewIssuedUtc;
                    model.Properties.ExpiresUtc = _renewExpiresUtc;

                    string cookieValue = Options.TicketDataFormat.Protect(model);

                    if (model.Properties.IsPersistent)
                    {
                        cookieOptions.Expires = _renewExpiresUtc.ToUniversalTime().DateTime;
                    }

                    Response.Cookies.Append(
                        Options.CookieName,
                        cookieValue,
                        cookieOptions);
                }

                Response.Headers.Set(
                    HeaderNameCacheControl,
                    HeaderValueNoCache);

                Response.Headers.Set(
                    HeaderNamePragma,
                    HeaderValueNoCache);

                Response.Headers.Set(
                    HeaderNameExpires,
                    HeaderValueMinusOne);

                bool shouldLoginRedirect = shouldSignin && !string.IsNullOrEmpty(Options.LoginPath) && string.Equals(Request.Path, Options.LoginPath, StringComparison.OrdinalIgnoreCase);
                bool shouldLogoutRedirect = shouldSignout && !string.IsNullOrEmpty(Options.LogoutPath) && string.Equals(Request.Path, Options.LogoutPath, StringComparison.OrdinalIgnoreCase);

                if ((shouldLoginRedirect || shouldLogoutRedirect) && Response.StatusCode == 200)
                {
                    IReadableStringCollection query = Request.Query;
                    string redirectUri = query.Get(Options.ReturnUrlParameter ?? CookieAuthenticationDefaults.ReturnUrlParameter);
                    if (!string.IsNullOrWhiteSpace(redirectUri)
                        && IsHostRelative(redirectUri))
                    {
                        Response.Redirect(redirectUri);
                    }
                }
            }
        }

        private static bool IsHostRelative(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            if (path.Length == 1)
            {
                return path[0] == '/';
            }
            return path[0] == '/' && path[1] != '/' && path[1] != '\\';
        }

        protected override Task ApplyResponseChallengeAsync()
        {
            _logger.WriteVerbose("ApplyResponseChallenge");
            if (Response.StatusCode != 401 || string.IsNullOrEmpty(Options.LoginPath))
            {
                return Task.FromResult<object>(null);
            }

            AuthenticationResponseChallenge challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

            if (challenge != null)
            {
                string baseUri = Request.Scheme + "://" + Request.Host + Request.PathBase;

                string currentUri = WebUtilities.AddQueryString(
                    Request.PathBase + Request.Path,
                    Request.QueryString);

                string loginUri = WebUtilities.AddQueryString(
                    baseUri + Options.LoginPath,
                    Options.ReturnUrlParameter ?? CookieAuthenticationDefaults.ReturnUrlParameter,
                    currentUri);

                Response.Redirect(loginUri);
            }

            return Task.FromResult<object>(null);
        }
    }
}
