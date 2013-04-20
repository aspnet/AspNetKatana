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
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.ModelSerializer;
using Owin.Types;
using Owin.Types.Extensions;
using Owin.Types.Helpers;

namespace Microsoft.Owin.Security.Forms
{
    using AuthenticateFunc = Func<string[], Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object>, object, Task>;

    internal class FormsAuthenticationContext
    {
        private const string IssuedUtcKey = ".issued";
        private const string ExpiresUtcKey = ".expires";
        private const string IsPersistentKey = ".persistent";
        private const string UtcDateTimeFormat = "r";

        private static readonly Action<object> ApplyResponseDelegate = obj => ((FormsAuthenticationContext)obj).ApplyResponse();

        private readonly FormsAuthenticationOptions _options;
        private readonly IDictionary<string, object> _description;
        private readonly IProtectionHandler<TicketModel> _modelProtection;
        private OwinRequest _request;
        private OwinResponse _response;
        private SecurityHelper _helper;
        private AuthenticateFunc _chainAuthenticate;
        private string _requestPathBase;
        private string _requestPath;

        private Task<ClaimsIdentity> _getIdentity;
        private bool _getIdentityInitialized;
        private object _getIdentitySyncLock;
        private IDictionary<string, string> _getIdentityExtra;

        private bool _applyResponse;
        private bool _applyResponseInitialized;
        private object _applyResponseSyncLock;

        private bool _shouldRenew;
        private DateTimeOffset _renewIssuedUtc;
        private DateTimeOffset _renewExpiresUtc;

        public FormsAuthenticationContext(FormsAuthenticationOptions options, IDictionary<string, object> description, IProtectionHandler<TicketModel> modelProtection, IDictionary<string, object> env)
        {
            _options = options;
            _description = description;
            _modelProtection = modelProtection;
            _request = new OwinRequest(env);
            _response = new OwinResponse(env);
            _helper = new SecurityHelper(env);
        }

        public async Task Initialize()
        {
            _chainAuthenticate = _request.AuthenticateDelegate;
            _request.AuthenticateDelegate = Authenticate;

            _requestPath = _request.Path;
            _requestPathBase = _request.PathBase;
            _request.OnSendingHeaders(ApplyResponseDelegate, this);

            if (_options.AuthenticationMode == AuthenticationMode.Active)
            {
                await ApplyIdentity();
            }
        }

        public void Teardown()
        {
            ApplyResponse();
        }

        private async Task ApplyIdentity()
        {
            IIdentity identity = await GetIdentity();
            if (identity != null)
            {
                _helper.AddUserIdentity(identity);
            }
        }

        private async Task Authenticate(
            string[] authenticationTypes,
            Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object> callback,
            object state)
        {
            if (authenticationTypes == null)
            {
                callback(null, null, _description, state);
            }
            else if (authenticationTypes.Contains(_options.AuthenticationType, StringComparer.Ordinal))
            {
                IIdentity identity = await GetIdentity();
                if (identity != null)
                {
                    callback(identity, _getIdentityExtra, _description, state);
                }
            }
            if (_chainAuthenticate != null)
            {
                await _chainAuthenticate(authenticationTypes, callback, state);
            }
        }

        private Task<ClaimsIdentity> GetIdentity()
        {
            return LazyInitializer.EnsureInitialized(
                ref _getIdentity,
                ref _getIdentityInitialized,
                ref _getIdentitySyncLock,
                GetIdentityOnce);
        }

        private async Task<ClaimsIdentity> GetIdentityOnce()
        {
            try
            {
                IDictionary<string, string> cookies = _request.GetCookies();
                string cookie;
                if (!cookies.TryGetValue(_options.CookieName, out cookie))
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

                if (issuedUtc != null && expiresUtc != null && _options.SlidingExpiration)
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

                var identity = model.Identity;

                if (_options.Provider != null)
                {
                    var command = new FormsValidateIdentityContext(identity, _getIdentityExtra);
                    await _options.Provider.ValidateIdentity(command);
                    identity = command.Identity;
                }

                _getIdentityExtra = model.Extra;
                return identity;
            }
            catch (Exception ex)
            {
                // TODO: trace
                return null;
            }
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

        private bool ApplyResponse()
        {
            return LazyInitializer.EnsureInitialized(
                ref _applyResponse,
                ref _applyResponseInitialized,
                ref _applyResponseSyncLock,
                ApplyResponseOnce);
        }

        private bool ApplyResponseOnce()
        {
            ApplyResponseGrant();
            ApplyResponseChallenge();
            return default(bool);
        }

        private void ApplyResponseGrant()
        {
            var signin = _helper.LookupSignin(_options.AuthenticationType);
            var shouldSignin = signin != null;
            var shouldSignout = _helper.LookupSignout(_options.AuthenticationType, _options.AuthenticationMode);

            if (shouldSignin || shouldSignout || _shouldRenew)
            {
                var cookieOptions = new CookieOptions
                {
                    Domain = _options.CookieDomain,
                    HttpOnly = _options.CookieHttpOnly,
                    Path = _options.CookiePath ?? "/",
                    Secure = _options.CookieSecure,
                };

                if (shouldSignin)
                {
                    var identity = signin.Item1;
                    var extra = signin.Item2;

                    var context = new FormsResponseSignInContext(
                        _response.Dictionary,
                        _options.AuthenticationType,
                        new ClaimsIdentity(identity),
                        extra);

                    var issuedUtc = DateTimeOffset.UtcNow;
                    var expiresUtc = issuedUtc.Add(_options.ExpireTimeSpan);

                    extra[IssuedUtcKey] = issuedUtc.ToString(UtcDateTimeFormat, CultureInfo.InvariantCulture);
                    extra[ExpiresUtcKey] = expiresUtc.ToString(UtcDateTimeFormat, CultureInfo.InvariantCulture);

                    _options.Provider.ResponseSignIn(context);

                    if (context.Extra.ContainsKey(IsPersistentKey))
                    {
                        cookieOptions.Expires = expiresUtc.ToUniversalTime().DateTime;
                    }

                    var model = new TicketModel(context.Identity, context.Extra);
                    var cookieValue = _modelProtection.ProtectModel(model);

                    _response.AddCookie(
                        _options.CookieName,
                        cookieValue,
                        cookieOptions);
                }
                else if (shouldSignout)
                {
                    _response.DeleteCookie(
                        _options.CookieName,
                        cookieOptions);
                }
                else if (_shouldRenew)
                {
                    // The call to GetIdentity should always be synchronous if this flag is set.
                    // The Result property is called instead of using _getIdentity property only
                    // to avoid a race condition induced by incorrectly written end-user code.

                    var identity = GetIdentity().Result;
                    var extra = _getIdentityExtra;
                    extra[IssuedUtcKey] = _renewIssuedUtc.ToString(UtcDateTimeFormat, CultureInfo.InvariantCulture);
                    extra[ExpiresUtcKey] = _renewExpiresUtc.ToString(UtcDateTimeFormat, CultureInfo.InvariantCulture);

                    var model = new TicketModel(identity, extra);
                    var cookieValue = _modelProtection.ProtectModel(model);

                    if (extra.ContainsKey(IsPersistentKey))
                    {
                        cookieOptions.Expires = _renewExpiresUtc.ToUniversalTime().DateTime;
                    }

                    _response.AddCookie(
                       _options.CookieName,
                       cookieValue,
                       cookieOptions);
                }

                bool shouldLoginRedirect = shouldSignin && !string.IsNullOrEmpty(_options.LoginPath) && string.Equals(_requestPath, _options.LoginPath, StringComparison.OrdinalIgnoreCase);
                bool shouldLogoutRedirect = shouldSignout && !string.IsNullOrEmpty(_options.LogoutPath) && string.Equals(_requestPath, _options.LogoutPath, StringComparison.OrdinalIgnoreCase);

                if ((shouldLoginRedirect || shouldLogoutRedirect) && _response.StatusCode == 200)
                {
                    IDictionary<string, string[]> query = _request.GetQuery();
                    string[] redirectUri;
                    if (query.TryGetValue("redirect_uri", out redirectUri) && redirectUri != null && redirectUri.Length == 1)
                    {
                        // TODO: safe redirect rules
                        _response.StatusCode = 302;
                        _response.AddHeader("Location", redirectUri[0]);
                    }
                }
            }
        }

        private void ApplyResponseChallenge()
        {
            if (_response.StatusCode != 401 || string.IsNullOrEmpty(_options.LoginPath))
            {
                return;
            }

            var challenge = _helper.LookupChallenge(_options.AuthenticationType, _options.AuthenticationMode);

            if (challenge != null)
            {
                string prefix = _request.Scheme + "://" + _request.Host + _request.PathBase;

                string queryString = _request.QueryString;

                string redirectUri = string.IsNullOrEmpty(queryString) 
                    ? prefix + _request.Path 
                    : prefix + _request.Path + "?" + queryString;

                string location = prefix + _options.LoginPath + "?redirect_uri=" + Uri.EscapeDataString(redirectUri);

                _response.StatusCode = 302;
                _response.SetHeader("Location", location);
            }
        }
    }
}
