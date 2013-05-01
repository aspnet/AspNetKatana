// <copyright file="FormsAuthenticationContext.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Infrastructure;

namespace Microsoft.Owin.Security.Forms
{
    internal class FormsAuthenticationHandler : AuthenticationHandler<FormsAuthenticationOptions>
    {
        private bool _shouldRenew;
        private DateTimeOffset _renewIssuedUtc;
        private DateTimeOffset _renewExpiresUtc;

        protected override async Task<AuthenticationTicket> AuthenticateCore()
        {
            IDictionary<string, string> cookies = Request.GetCookies();
            string cookie;
            if (!cookies.TryGetValue(Options.CookieName, out cookie))
            {
                return null;
            }

            var model = Options.TicketDataHandler.Unprotect(cookie);

            if (model == null)
            {
                return null;
            }

            DateTimeOffset currentUtc = DateTimeOffset.UtcNow;
            DateTimeOffset? issuedUtc = model.Extra.IssuedUtc;
            DateTimeOffset? expiresUtc = model.Extra.ExpiresUtc;

            if (expiresUtc != null && expiresUtc.Value < currentUtc)
            {
                return null;
            }

            if (issuedUtc != null && expiresUtc != null && Options.SlidingExpiration)
            {
                var timeElapsed = currentUtc.Subtract(issuedUtc.Value);
                var timeRemaining = expiresUtc.Value.Subtract(currentUtc);

                if (timeRemaining < timeElapsed)
                {
                    _shouldRenew = true;
                    _renewIssuedUtc = currentUtc;
                    var timeSpan = expiresUtc.Value.Subtract(issuedUtc.Value);
                    _renewExpiresUtc = currentUtc.Add(timeSpan);
                }
            }

            var context = new FormsValidateIdentityContext(model);

            await Options.Provider.ValidateIdentity(context);

            return new AuthenticationTicket(context.Identity, context.Extra);
        }

        protected override async Task ApplyResponseGrant()
        {
            var signin = Helper.LookupSignin(Options.AuthenticationType);
            var shouldSignin = signin != null;
            var signout = Helper.LookupSignout(Options.AuthenticationType, Options.AuthenticationMode);
            var shouldSignout = signout != null;

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
                    cookieOptions.Secure = string.Equals(Request.Scheme, "HTTPS", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    cookieOptions.Secure = Options.CookieSecure == CookieSecureOption.Always;
                }

                if (shouldSignin)
                {
                    var context = new FormsResponseSignInContext(
                        Response.Environment,
                        Options.AuthenticationType,
                        signin.Identity,
                        signin.Extra);

                    var issuedUtc = DateTimeOffset.UtcNow;
                    var expiresUtc = issuedUtc.Add(Options.ExpireTimeSpan);

                    context.Extra.IssuedUtc = issuedUtc;
                    context.Extra.ExpiresUtc = expiresUtc;

                    Options.Provider.ResponseSignIn(context);

                    if (context.Extra.IsPersistent)
                    {
                        cookieOptions.Expires = expiresUtc.ToUniversalTime().DateTime;
                    }

                    var model = new AuthenticationTicket(context.Identity, context.Extra.Properties);
                    var cookieValue = Options.TicketDataHandler.Protect(model);

                    Response.AddCookie(
                        Options.CookieName,
                        cookieValue,
                        cookieOptions);
                }
                else if (shouldSignout)
                {
                    Response.DeleteCookie(
                        Options.CookieName,
                        cookieOptions);
                }
                else if (_shouldRenew)
                {
                    var model = await Authenticate();

                    model.Extra.IssuedUtc = _renewIssuedUtc;
                    model.Extra.ExpiresUtc = _renewExpiresUtc;

                    var cookieValue = Options.TicketDataHandler.Protect(model);

                    if (model.Extra.IsPersistent)
                    {
                        cookieOptions.Expires = _renewExpiresUtc.ToUniversalTime().DateTime;
                    }

                    Response.AddCookie(
                       Options.CookieName,
                       cookieValue,
                       cookieOptions);
                }

                bool shouldLoginRedirect = shouldSignin && !string.IsNullOrEmpty(Options.LoginPath) && string.Equals(Request.Path, Options.LoginPath, StringComparison.OrdinalIgnoreCase);
                bool shouldLogoutRedirect = shouldSignout && !string.IsNullOrEmpty(Options.LogoutPath) && string.Equals(Request.Path, Options.LogoutPath, StringComparison.OrdinalIgnoreCase);

                if ((shouldLoginRedirect || shouldLogoutRedirect) && Response.StatusCode == 200)
                {
                    IDictionary<string, string[]> query = Request.GetQuery();
                    string[] redirectUri;
                    if (query.TryGetValue(Options.ReturnUrlParameter ?? Constants.DefaultReturnUrlParameter, out redirectUri) &&
                        redirectUri != null &&
                        redirectUri.Length == 1)
                    {
                        // TODO: safe redirect rules
                        Response.Redirect(redirectUri[0]);
                    }
                }
            }
        }

        protected override async Task ApplyResponseChallenge()
        {
            if (Response.StatusCode != 401 || string.IsNullOrEmpty(Options.LoginPath))
            {
                return;
            }

            var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

            if (challenge != null)
            {
                string baseUri = Request.Scheme + "://" + Request.Host + Request.PathBase;

                string currentUri = WebUtils.AddQueryString(
                    baseUri + Request.Path,
                    Request.QueryString);

                string loginUri = WebUtils.AddQueryString(
                    baseUri + Options.LoginPath,
                    Options.ReturnUrlParameter ?? Constants.DefaultReturnUrlParameter,
                    currentUri);

                Response.Redirect(loginUri);
            }
        }
    }
}
