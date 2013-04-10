// <copyright file="FederationAuthenticationContext.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Globalization;
using System.IdentityModel.Configuration;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Services;
using System.IdentityModel.Services.Configuration;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Owin.Types;
using Owin.Types.Helpers;

namespace Microsoft.Owin.Security.Federation
{
    internal class FederationAuthenticationContext
    {
        private readonly FederationAuthenticationOptions _options;
        private readonly FederationConfiguration _federationConfiguration;
        private OwinRequest _request;
        private OwinResponse _response;
        private Func<string[], Action<IIdentity, object>, object, Task> _chainGetIdentities;

        private Task<IIdentity> _getIdentity;
        private bool _getIdentityInitialized;
        private object _getIdentitySyncLock;

        private bool _applyChallenge;
        private bool _applyChallengeInitialized;
        private object _applyChallengeSyncLock;

        public FederationAuthenticationContext(
            FederationAuthenticationOptions options,
            FederationConfiguration federationConfiguration,
            IDictionary<string, object> env)
        {
            _options = options;
            _federationConfiguration = federationConfiguration;
            _request = new OwinRequest(env);
            _response = new OwinResponse(env);
        }

        public async Task Initialize()
        {
            _chainGetIdentities = _request.GetIdentitiesDelegate;
            _request.GetIdentitiesDelegate = GetIdentities;

            _request.OnSendingHeaders(ApplyResponseDelegate, this);

            if (_options.AuthenticationMode == AuthenticationMode.Active)
            {
                await ApplyIdentity();
            }
        }

        public async Task<bool> Invoke()
        {
            return await InvokeReplyPath();
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
                // _request.User = IdentityUtils.Join(identity, _request.User);
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

        private static readonly Action<object> ApplyResponseDelegate = obj => ((FederationAuthenticationContext)obj).ApplyResponse();

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

            IPrincipal challenge = _response.Challenge;
            ClaimsIdentity challengeIdentity = challenge == null ? null : new ClaimsPrincipal(_response.Challenge)
                .Identities
                .SingleOrDefault(x => x.AuthenticationType == _options.AuthenticationType);

            bool activeChallenge = challenge == null && _options.AuthenticationMode == AuthenticationMode.Active;
            bool passiveChallenge = challenge != null && challengeIdentity != null;

            if (activeChallenge || passiveChallenge)
            {
                string issuer = _federationConfiguration.WsFederationConfiguration.Issuer;
                string realm = _federationConfiguration.WsFederationConfiguration.Realm;
                var message = new SignInRequestMessage(new Uri(issuer), realm);
                message.Freshness = _federationConfiguration.WsFederationConfiguration.Freshness;
                message.CurrentTime = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture) + "Z";
                message.AuthenticationType = _federationConfiguration.WsFederationConfiguration.AuthenticationType;
                message.HomeRealm = _federationConfiguration.WsFederationConfiguration.HomeRealm;
                message.Policy = _federationConfiguration.WsFederationConfiguration.Policy;
                message.Reply = _federationConfiguration.WsFederationConfiguration.Reply;
                message.Resource = _federationConfiguration.WsFederationConfiguration.Resource;
                message.Request = _federationConfiguration.WsFederationConfiguration.Request;
                message.RequestPtr = _federationConfiguration.WsFederationConfiguration.RequestPtr;

                string prefix = _request.Scheme + "://" + _request.Host + _request.PathBase;

                string queryString = _request.QueryString;
                string redirectUri = string.IsNullOrEmpty(queryString) ?
                                                                           prefix + _request.Path :
                                                                                                      prefix + _request.Path + "?" + queryString;

                message.Context = redirectUri;

                _response.StatusCode = 302;
                _response.SetHeader("Location", message.RequestUrl);
            }
        }

        public async Task<bool> InvokeReplyPath()
        {
            if (_options.ReturnPath != null &&
                String.Equals(_options.ReturnPath, _request.Path, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(_request.Method, "POST", StringComparison.OrdinalIgnoreCase))
            {
                var form = new NameValueCollection();
                using (var reader = new StreamReader(_request.Body))
                {
                    string text = await reader.ReadToEndAsync();
                    OwinHelpers.ParseDelimited(
                        text,
                        new[] { '&' },
                        (a, b, c) => ((NameValueCollection)c).Add(a, b),
                        form);
                }

                WSFederationMessage message = WSFederationMessage.CreateFromNameValueCollection(
                    new Uri(_federationConfiguration.WsFederationConfiguration.Realm),
                    form);

                var signIn = message as SignInResponseMessage;
                if (signIn != null)
                {
                    XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(signIn.Result), XmlDictionaryReaderQuotas.Max);
                    var federationSerializer = new WSFederationSerializer(reader);
                    var serializationContext = new WSTrustSerializationContext(_federationConfiguration.IdentityConfiguration.SecurityTokenHandlerCollectionManager);
                    RequestSecurityTokenResponse securityTokenResponse = federationSerializer.CreateResponse(signIn, serializationContext);
                    string xml = securityTokenResponse.RequestedSecurityToken.SecurityTokenXml.OuterXml;

                    SecurityToken securityToken = ReadToken(xml);

                    var securityTokenReceivedContext = new SecurityTokenReceivedContext(securityToken);
                    await _options.Provider.SecurityTokenReceived(securityTokenReceivedContext);

                    ClaimsPrincipal principal = AuthenticateToken(securityToken, _request.Uri.AbsoluteUri);

                    var securityTokenValidatedContext = new SecurityTokenValidatedContext(principal);
                    await _options.Provider.SecurityTokenValidated(securityTokenValidatedContext);

                    if (!string.IsNullOrEmpty(_options.SigninAsAuthenticationType))
                    {
                        _response.Grant = new ClaimsPrincipal(new ClaimsIdentity(securityTokenValidatedContext.ClaimsPrincipal.Claims, _options.SigninAsAuthenticationType));
                    }
                    _response.StatusCode = 302;
                    _response.SetHeader("Location", signIn.Context);
                    return true;
                }
            }
            return false;
        }

        private SecurityToken ReadToken(string text)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(text)))
            {
                reader.MoveToContent();

                SecurityTokenHandlerCollection handlers = _federationConfiguration.IdentityConfiguration.SecurityTokenHandlers;
                return handlers.CanReadToken(reader) ? handlers.ReadToken(reader) : null;
            }
        }

        private ClaimsPrincipal AuthenticateToken(SecurityToken token, string resourceName)
        {
            IdentityConfiguration identityConfiguration = _federationConfiguration.IdentityConfiguration;
            var incomingPrincipal = new ClaimsPrincipal(identityConfiguration.SecurityTokenHandlers.ValidateToken(token));
            return identityConfiguration.ClaimsAuthenticationManager.Authenticate(resourceName, incomingPrincipal);
        }
    }
}
