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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.Serialization;
using Owin.Types;
using Owin.Types.Extensions;
using Owin.Types.Helpers;

namespace Microsoft.Owin.Security.Forms
{
    using AuthenticateFunc = Func<string[], Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object>, object, Task>;

    internal class FormsAuthenticationContext
    {
        private static readonly Action<object> ApplyResponseDelegate = obj => ((FormsAuthenticationContext)obj).ApplyResponse();

        private readonly FormsAuthenticationOptions _options;
        private readonly IDictionary<string, object> _description;
        private OwinRequest _request;
        private OwinResponse _response;
        private SecurityHelper _helper;
        private AuthenticateFunc _chainAuthenticate;
        private string _requestPathBase;
        private string _requestPath;

        private Task<IIdentity> _getIdentity;
        private bool _getIdentityInitialized;
        private object _getIdentitySyncLock;
        private IDictionary<string, string> _getIdentityExtra;

        private bool _applyResponse;
        private bool _applyResponseInitialized;
        private object _applyResponseSyncLock;

        public FormsAuthenticationContext(FormsAuthenticationOptions options, IDictionary<string, object> description, IDictionary<string, object> env)
        {
            _options = options;
            _description = description;
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

        private Task<IIdentity> GetIdentity()
        {
            return LazyInitializer.EnsureInitialized(
                ref _getIdentity,
                ref _getIdentityInitialized,
                ref _getIdentitySyncLock,
                GetIdentityOnce);
        }

        private async Task<IIdentity> GetIdentityOnce()
        {
            try
            {
                IDictionary<string, string> cookies = _request.GetCookies();
                string cookie;
                if (!cookies.TryGetValue(_options.CookieName, out cookie))
                {
                    return null;
                }

                byte[] protectedData = Convert.FromBase64String(cookie);
                byte[] userData = _options.DataProtection.Unprotect(protectedData);
#if DEBUG
                string peek = Encoding.UTF8.GetString(userData);
#endif
                DataModel formsData = DataModelSerialization.Deserialize(userData);
                IIdentity identity = formsData.Principal.Identity;

                _getIdentityExtra = formsData.Extra;

                if (_options.Provider != null)
                {
                    var command = new FormsValidateIdentityContext(identity, _getIdentityExtra);
                    await _options.Provider.ValidateIdentity(command);
                    identity = command.Identity;
                }

                return identity;
            }
            catch (Exception ex)
            {
                // TODO: trace
                return null;
            }
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

            if (shouldSignin || shouldSignout)
            {
                // TODO: verify we are on login/logout/etc path?
                // TODO: need a "set expiration" flag?
                var cookieOptions = new CookieOptions
                {
                    Domain = _options.CookieDomain,
                    HttpOnly = _options.CookieHttpOnly,
                    Path = _options.CookiePath ?? "/",
                    Secure = _options.CookieSecure,
                };

                if (shouldSignin)
                {
                    var context = new FormsResponseSignInContext(
                        _response.Dictionary,
                        _options.AuthenticationType,
                        signin.Item1,
                        signin.Item2);

                    _options.Provider.ResponseSignIn(context);

                    var formsData = new DataModel(
                        new ClaimsPrincipal(context.Identity),
                        context.Extra);

                    byte[] userData = DataModelSerialization.Serialize(formsData);

                    byte[] protectedData = _options.DataProtection.Protect(userData);
                    _response.AddCookie(
                        _options.CookieName,
                        Convert.ToBase64String(protectedData),
                        cookieOptions);
                }
                else
                {
                    _response.DeleteCookie(
                        _options.CookieName,
                        cookieOptions);
                }

                bool shouldLoginRedirect = shouldSignin && !string.IsNullOrEmpty(_options.LoginPath) && string.Equals(_requestPath, _options.LoginPath, StringComparison.OrdinalIgnoreCase);
                bool shouldLogoutRedirect = shouldSignout && !string.IsNullOrEmpty(_options.LogoutPath) && string.Equals(_requestPath, _options.LogoutPath, StringComparison.OrdinalIgnoreCase);

                if (shouldLoginRedirect || shouldLogoutRedirect)
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

                string redirectUri = string.IsNullOrEmpty(queryString) ?
                                                                           prefix + _request.Path :
                                                                                                      prefix + _request.Path + "?" + queryString;

                string location = prefix + _options.LoginPath + "?redirect_uri=" + Uri.EscapeDataString(redirectUri);

                _response.StatusCode = 302;
                _response.SetHeader("Location", location);
            }
        }
    }
}
