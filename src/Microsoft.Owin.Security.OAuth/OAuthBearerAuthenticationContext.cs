// <copyright file="OAuthBearerAuthenticationContext.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.Serialization;
using Owin.Types;

namespace Microsoft.Owin.Security.OAuth
{
    internal class OAuthBearerAuthenticationContext
    {
        private static readonly Action<object> ApplyResponseDelegate = obj => ((OAuthBearerAuthenticationContext)obj).ApplyResponse();

        private readonly OAuthBearerAuthenticationOptions _options;
        private readonly string _challenge;
        private readonly IDictionary<string, object> _description;
        private OwinRequest _request;
        private OwinResponse _response;
        private SecurityHelper _helper;
        private Func<string[], Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object>, object, Task> _chainAuthenticate;
        private string _requestPathBase;
        private string _requestPath;

        private Task<IIdentity> _getIdentity;
        private bool _getIdentityInitialized;
        private object _getIdentitySyncLock;
        private IDictionary<string, string> _getIdentityExtra;

        private bool _applyResponse;
        private bool _applyResponseInitialized;
        private object _applyResponseSyncLock;

        public OAuthBearerAuthenticationContext(OAuthBearerAuthenticationOptions options, string challenge, IDictionary<string, object> description, IDictionary<string, object> env)
        {
            _options = options;
            _challenge = challenge;
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
                string authorization = _request.GetHeader("Authorization");

                if (authorization == null || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                string encodedData = authorization.Substring("Bearer ".Length).Trim().Replace('-', '+').Replace('_', '/');
                byte[] protectedData = Convert.FromBase64String(encodedData);
                byte[] userData = _options.DataProtection.Unprotect(protectedData);
                DataModel model = DataModelSerialization.Deserialize(userData);

                var command = new OAuthValidateIdentityContext(model.Principal.Identity, model.Extra);
                if (_options.Provider != null)
                {
                    await _options.Provider.ValidateIdentity(command);
                }

                _getIdentityExtra = command.Extra;
                return command.Identity;
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
        }

        private void ApplyResponseChallenge()
        {
            if (_response.StatusCode != 401)
            {
                return;
            }

            Tuple<string[], IDictionary<string, string>> challenge = _helper.LookupChallenge(_options.AuthenticationType, _options.AuthenticationMode);

            if (challenge != null)
            {
                _response.AddHeader("WWW-Authenticate", _challenge);
            }
        }
    }
}
