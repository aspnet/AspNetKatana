// <copyright file="FacebookAuthenticationContext.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Infrastructure;
using Newtonsoft.Json.Linq;
using Owin.Types;
using Owin.Types.Extensions;
using Owin.Types.Helpers;

namespace Microsoft.Owin.Security.Facebook
{
    internal class FacebookAuthenticationContext
    {
        private static readonly Action<object> ApplyResponseDelegate = obj => ((FacebookAuthenticationContext)obj).ApplyResponse();

        private readonly FacebookAuthenticationOptions _options;
        private readonly IDictionary<string, object> _description;
        private readonly IProtectionHandler<IDictionary<string, string>> _extraProtectionHandler;

        private OwinRequest _request;
        private OwinResponse _response;
        private SecurityHelper _helper;
        private Func<string[], Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object>, object, Task> _chainAuthenticate;
        private string _requestPathBase;

        private Task<ClaimsIdentity> _getIdentity;
        private bool _getIdentityInitialized;
        private object _getIdentitySyncLock;
        private IDictionary<string, string> _getIdentityExtra;

        private bool _applyChallenge;
        private bool _applyChallengeInitialized;
        private object _applyChallengeSyncLock;


        public FacebookAuthenticationContext(
            FacebookAuthenticationOptions options,
            IDictionary<string, object> description,
            IDictionary<string, object> env, 
            IProtectionHandler<IDictionary<string, string>> extraProtectionHandler)
        {
            _options = options;
            _description = description;
            _extraProtectionHandler = extraProtectionHandler;
            _request = new OwinRequest(env);
            _response = new OwinResponse(env);
            _helper = new SecurityHelper(env);
        }

        public async Task Initialize()
        {
            _chainAuthenticate = _request.AuthenticateDelegate;
            _request.AuthenticateDelegate = Authenticate;

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
                    callback(identity, null, _description, state);
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
                string code = null;
                string state = null;

                IDictionary<string, string[]> query = _request.GetQuery();
                string[] values;
                if (query.TryGetValue("code", out values) && values != null && values.Length == 1)
                {
                    code = values[0];
                }
                if (query.TryGetValue("state", out values) && values != null && values.Length == 1)
                {
                    state = values[0];
                }
                var extra = _extraProtectionHandler.UnprotectModel(state);
                if (extra == null)
                {
                    return null;
                }

                string tokenEndpoint =
                    "https://graph.facebook.com/oauth/access_token";

                string requestPrefix = _request.Scheme + "://" + _request.Host;
                string redirectUri = requestPrefix + _requestPathBase + _options.ReturnEndpointPath;

                string tokenRequest = "grant_type=authorization_code" +
                    "&code=" + Uri.EscapeDataString(code) +
                    "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
                    "&client_id=" + Uri.EscapeDataString(_options.AppId) +
                    "&client_secret=" + Uri.EscapeDataString(_options.AppSecret);

                WebRequest webRequest = WebRequest.Create(tokenEndpoint + "?" + tokenRequest);
                WebResponse webResponse = await webRequest.GetResponseAsync();

                var form = new NameValueCollection();
                using (var reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    string text = await reader.ReadToEndAsync();
                    OwinHelpers.ParseDelimited(
                        text,
                        new[] { '&' },
                        (a, b, c) => ((NameValueCollection)c).Add(a, b),
                        form);
                }
                string accessToken = form["access_token"];
                string expires = form["expires"];

                string graphApiEndpoint =
                    "https://graph.facebook.com/me";

                webRequest = WebRequest.Create(graphApiEndpoint + "?access_token=" + Uri.EscapeDataString(accessToken));
                webResponse = await webRequest.GetResponseAsync();
                JObject user;
                using (var reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    user = JObject.Parse(await reader.ReadToEndAsync());
                }

                var context = new FacebookAuthenticatedContext(_request.Dictionary, user, accessToken);
                context.Identity = new ClaimsIdentity(
                    new[]
                    {
                        new Claim("urn:facebook:id", context.Id),
                        new Claim("urn:facebook:name", context.Name),
                        new Claim("urn:facebook:link", context.Link),
                        new Claim("urn:facebook:username", context.Username),
                        new Claim("urn:facebook:email", context.Email)
                    },
                    _options.AuthenticationType,
                    "urn:facebook:name",
                    ClaimTypes.Role);

                context.Extra = extra;

                await _options.Provider.Authenticated(context);

                _getIdentityExtra = context.Extra;
                return context.Identity;
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
                ref _applyChallenge,
                ref _applyChallengeInitialized,
                ref _applyChallengeSyncLock,
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
        }

        private void ApplyResponseChallenge()
        {
            if (_response.StatusCode != 401)
            {
                return;
            }

            var challenge = _helper.LookupChallenge(_options.AuthenticationType, _options.AuthenticationMode);

            if (challenge != null)
            {
                string requestPrefix = _request.Scheme + "://" + _request.Host;

                string currentQueryString = _request.QueryString;
                string currentUri = string.IsNullOrEmpty(currentQueryString)
                    ? requestPrefix + _request.PathBase + _request.Path
                    : requestPrefix + _request.PathBase + _request.Path + "?" + currentQueryString;

                string redirectUri = requestPrefix + _requestPathBase + _options.ReturnEndpointPath;

                var extra = challenge.Item2;
                if (extra == null)
                {
                    extra = new Dictionary<string, string>(StringComparer.Ordinal);
                }

                string extraRedirectUri;
                if (extra.TryGetValue("RedirectUri", out extraRedirectUri))
                {
                    redirectUri = extraRedirectUri;
                }
                else
                {
                    extra["RedirectUri"] = currentUri;
                }

                var state = _extraProtectionHandler.ProtectModel(extra);

                string authorizationEndpoint =
                    "https://www.facebook.com/dialog/oauth" +
                        "?response_type=code" +
                        "&client_id=" + Uri.EscapeDataString(_options.AppId) +
                        "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
                        "&scope=" + Uri.EscapeDataString("email") +
                        "&state=" + Uri.EscapeDataString(state);

                _response.StatusCode = 302;
                _response.SetHeader("Location", authorizationEndpoint);
            }
        }

        public async Task<bool> Invoke()
        {
            return await InvokeReplyPath();
        }

        public async Task<bool> InvokeReplyPath()
        {
            if (_options.ReturnEndpointPath != null &&
                String.Equals(_options.ReturnEndpointPath, _request.Path, StringComparison.OrdinalIgnoreCase))
            {
                // TODO: error responses

                var identity = await GetIdentity();

                var context = new FacebookReturnEndpointContext(_request.Dictionary, identity, _getIdentityExtra);
                context.SignInAsAuthenticationType = _options.SignInAsAuthenticationType;
                string redirectUri;
                if (_getIdentityExtra.TryGetValue("RedirectUri", out redirectUri))
                {
                    context.RedirectUri = redirectUri;
                }

                await _options.Provider.ReturnEndpoint(context);

                if (context.SignInAsAuthenticationType != null &&
                    context.Identity != null)
                {
                    var signInIdentity = context.Identity;
                    if (!string.Equals(signInIdentity.AuthenticationType, context.SignInAsAuthenticationType, StringComparison.Ordinal))
                    {
                        signInIdentity = new ClaimsIdentity(signInIdentity.Claims, context.SignInAsAuthenticationType, signInIdentity.NameClaimType, signInIdentity.RoleClaimType);
                    }
                    _response.SignIn(new ClaimsPrincipal(signInIdentity), context.Extra);
                }

                if (!context.IsRequestCompleted && context.RedirectUri != null)
                {
                    _response.Redirect(context.RedirectUri);
                    context.RequestCompleted();
                }

                return context.IsRequestCompleted;
            }
            return false;
        }
    }
}
