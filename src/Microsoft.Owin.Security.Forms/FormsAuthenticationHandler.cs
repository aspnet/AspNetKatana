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
using System.Globalization;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Forms
{
    internal class FormsAuthenticationHandler : AuthenticationHandler<FormsAuthenticationOptions>
    {
        private const string IssuedUtcKey = ".issued";
        private const string ExpiresUtcKey = ".expires";
        private const string IsPersistentKey = ".persistent";
        private const string UtcDateTimeFormat = "r";
      
        private readonly IProtectionHandler<AuthenticationData> _modelProtection;

        private bool _shouldRenew;
        private DateTimeOffset _renewIssuedUtc;
        private DateTimeOffset _renewExpiresUtc;

        public FormsAuthenticationHandler(IProtectionHandler<AuthenticationData> modelProtection)
        {
            _modelProtection = modelProtection;
        }

        protected override async Task<AuthenticationData> AuthenticateCore()
        {
            IDictionary<string, string> cookies = Request.GetCookies();
            string cookie;
            if (!cookies.TryGetValue(Options.CookieName, out cookie))
            {
                return null;
            }

            var model = _modelProtection.UnprotectModel(cookie);

            if (model == null)
            {
                return null;
            }

            DateTimeOffset currentUtc = DateTimeOffset.UtcNow;
            DateTimeOffset? issuedUtc = ParseUtc(model.Extra, IssuedUtcKey);
            DateTimeOffset? expiresUtc = ParseUtc(model.Extra, ExpiresUtcKey);

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

            var command = new FormsValidateIdentityContext(model.Identity, model.Extra);

            if (Options.Provider != null)
            {
                await Options.Provider.ValidateIdentity(command);
            }

            return new AuthenticationData(command.Identity, command.Extra);
        }

        private DateTimeOffset? ParseUtc(IDictionary<string, string> extra, string key)
        {
            string value;
            if (extra.TryGetValue(key, out value))
            {
                DateTimeOffset dateTimeOffset;
                if (DateTimeOffset.TryParseExact(value, UtcDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTimeOffset))
                {
                    return dateTimeOffset;
                }
            }
            return null;
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
                    Secure = Options.CookieSecure,
                };

                if (shouldSignin)
                {                   
                    var context = new FormsResponseSignInContext(
                        Response.Environment,
                        Options.AuthenticationType,
                        signin.Identity,
                        signin.Extra);

                    var issuedUtc = DateTimeOffset.UtcNow;
                    var expiresUtc = issuedUtc.Add(Options.ExpireTimeSpan);

                    context.Extra[IssuedUtcKey] = issuedUtc.ToString(UtcDateTimeFormat, CultureInfo.InvariantCulture);
                    context.Extra[ExpiresUtcKey] = expiresUtc.ToString(UtcDateTimeFormat, CultureInfo.InvariantCulture);

                    Options.Provider.ResponseSignIn(context);

                    if (context.Extra.ContainsKey(IsPersistentKey))
                    {
                        cookieOptions.Expires = expiresUtc.ToUniversalTime().DateTime;
                    }

                    var model = new AuthenticationData(context.Identity, context.Extra);
                    var cookieValue = _modelProtection.ProtectModel(model);

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

                    model.Extra[IssuedUtcKey] = _renewIssuedUtc.ToString(UtcDateTimeFormat, CultureInfo.InvariantCulture);
                    model.Extra[ExpiresUtcKey] = _renewExpiresUtc.ToString(UtcDateTimeFormat, CultureInfo.InvariantCulture);

                    var cookieValue = _modelProtection.ProtectModel(model);

                    if (model.Extra.ContainsKey(IsPersistentKey))
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
                    if (query.TryGetValue("redirect_uri", out redirectUri) && redirectUri != null && redirectUri.Length == 1)
                    {
                        // TODO: safe redirect rules
                        Response.StatusCode = 302;
                        Response.AddHeader("Location", redirectUri[0]);
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
                string prefix = Request.Scheme + "://" + Request.Host + Request.PathBase;

                string queryString = Request.QueryString;

                string redirectUri = string.IsNullOrEmpty(queryString)
                    ? prefix + Request.Path
                    : prefix + Request.Path + "?" + queryString;

                string location = prefix + Options.LoginPath + "?redirect_uri=" + Uri.EscapeDataString(redirectUri);

                Response.StatusCode = 302;
                Response.SetHeader("Location", location);
            }
        }
    }
}
