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
        private SecurityHelper _helper;
        private OwinRequest _request;
        private OwinResponse _response;
        private Func<string[], Action<IIdentity, object>, object, Task> _chainGetIdentities;

        private Task<IIdentity> _getIdentity;
        private bool _getIdentityInitialized;
        private object _getIdentitySyncLock;

        private bool _applyChallenge;
        private bool _applyChallengeInitialized;
        private object _applyChallengeSyncLock;
        private string _requestPathBase;

        public FacebookAuthenticationContext(
            FacebookAuthenticationOptions options,
            IDictionary<string, object> env)
        {
            _options = options;
            _request = new OwinRequest(env);
            _response = new OwinResponse(env);
            _helper = new SecurityHelper(env);
        }

        public async Task Initialize()
        {
            _chainGetIdentities = _request.GetIdentitiesDelegate;
            _request.GetIdentitiesDelegate = GetIdentities;

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

        private async Task GetIdentities(string[] authenticationTypes, Action<IIdentity, object> callback, object state)
        {
            if (authenticationTypes == null)
            {
                callback(new ClaimsIdentity(_options.AuthenticationType), state);
            }
            else if (authenticationTypes.Contains(_options.AuthenticationType, StringComparer.Ordinal))
            {
                IIdentity identity = await GetIdentity();
                if (identity != null)
                {
                    callback(identity, state);
                }
            }
            if (_chainGetIdentities != null)
            {
                await _chainGetIdentities(authenticationTypes, callback, state);
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
                return null;
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

            SecurityHelperLookupResult challenge = _helper.LookupChallenge(_options.AuthenticationType, _options.AuthenticationMode);

            if (challenge.ShouldHappen)
            {
                string requestPrefix = _request.Scheme + "://" + _request.Host;

                string currentQueryString = _request.QueryString;
                string currentUri = string.IsNullOrEmpty(currentQueryString) ?
                                                                                 requestPrefix + _request.PathBase + _request.Path :
                                                                                                                                       requestPrefix + _request.PathBase + _request.Path + "?" + currentQueryString;

                string redirectUri = requestPrefix + _requestPathBase + _options.RedirectionEndpointPath;

                string authorizationEndpoint =
                    "https://www.facebook.com/dialog/oauth" +
                        "?response_type=code" +
                        "&client_id=" + Uri.EscapeDataString(_options.AppId) +
                        "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
                        "&scope=" + Uri.EscapeDataString("email") +
                        "&state=" + Uri.EscapeDataString(currentUri);

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
            if (_options.RedirectionEndpointPath != null &&
                String.Equals(_options.RedirectionEndpointPath, _request.Path, StringComparison.OrdinalIgnoreCase))
            {
                // TODO: error responses
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

                string tokenEndpoint =
                    "https://graph.facebook.com/oauth/access_token";

                string requestPrefix = _request.Scheme + "://" + _request.Host;
                string redirectUri = requestPrefix + _requestPathBase + _options.RedirectionEndpointPath;

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
                var context = new FacebookValidateLoginContext(_request.Dictionary, user, accessToken, state);
                await _options.Provider.ValidateLogin(context);
                if (context.SigninPrincipal != null)
                {
                    _response.Grant = context.SigninPrincipal;
                }
                if (context.RedirectUri != null)
                {
                    _response.StatusCode = 302;
                    _response.AddHeader("Location", context.RedirectUri);
                    return true;
                }
                return false;
            }
            return false;
        }
    }
}
