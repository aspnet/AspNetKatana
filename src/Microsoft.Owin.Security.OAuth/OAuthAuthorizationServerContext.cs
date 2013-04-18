// <copyright file="OAuthAuthorizationServerContext.cs" company="Microsoft Open Technologies, Inc.">
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
using System.IO;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth.Messages;
using Microsoft.Owin.Security.Serialization;
using Newtonsoft.Json;
using Owin.Types;
using Owin.Types.Extensions;
using Owin.Types.Helpers;

namespace Microsoft.Owin.Security.OAuth
{
    internal class OAuthAuthorizationServerContext
    {
        private static readonly Action<object> ApplyResponseDelegate = obj => ((OAuthAuthorizationServerContext)obj).ApplyResponse();

        private readonly Func<IDictionary<string, object>, Task> _next;
        private readonly OAuthAuthorizationServerOptions _options;
        private readonly string _requestPath;
        private OwinRequest _request;
        private OwinResponse _response;
        private SecurityHelper _helper;

        private bool _applyResponse;
        private bool _applyResponseInitialized;
        private object _applyResponseSyncLock;
        private AuthorizeRequest _authorizeRequest;

        public OAuthAuthorizationServerContext(Func<IDictionary<string, object>, Task> next, OAuthAuthorizationServerOptions options, IDictionary<string, object> env)
        {
            _next = next;
            _options = options;
            _request = new OwinRequest(env);
            _response = new OwinResponse(env);
            _helper = new SecurityHelper(env);
            _requestPath = _request.Path;
        }

        public async Task Initialize()
        {
            _request.OnSendingHeaders(ApplyResponseDelegate, this);
        }

        public void Teardown()
        {
            ApplyResponse();
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
            return default(bool);
        }

        private void ApplyResponseGrant()
        {
            Tuple<IIdentity, IDictionary<string, string>> signin = _helper.LookupSignin("Bearer");
            if (_authorizeRequest != null && signin != null)
            {
                var model = new DataModel(new ClaimsPrincipal(signin.Item1), signin.Item2);
                byte[] userData = DataModelSerialization.Serialize(model);
                byte[] protectedData = _options.DataProtection.Protect(userData);
                string text = Convert.ToBase64String(protectedData).Replace('+', '-').Replace('/', '_');

                string redirectUrl = _authorizeRequest.RedirectUri;
                bool hasQueryString = redirectUrl.IndexOf('?') != -1;

                if (_authorizeRequest.ResponseType == "code")
                {
                    redirectUrl +=
                        (hasQueryString ? "&code=" : "?code=") +
                            Uri.EscapeDataString(text) +
                            "&state=" +
                            Uri.EscapeDataString(_authorizeRequest.State);
                    _response.Redirect(redirectUrl);
                }
                else if (_authorizeRequest.ResponseType == "token")
                {
                }
            }
        }

        internal async Task<bool> TryInvoke()
        {
            if (!string.IsNullOrEmpty(_options.AuthorizeEndpointPath) && string.Equals(_options.AuthorizeEndpointPath, _requestPath, StringComparison.OrdinalIgnoreCase))
            {
                await InvokeAuthorizeEndpoint();
                return true;
            }
            if (!string.IsNullOrEmpty(_options.TokenEndpointPath) && string.Equals(_options.TokenEndpointPath, _requestPath, StringComparison.OrdinalIgnoreCase))
            {
                await InvokeTokenEndpoint();
                return true;
            }
            return false;
        }

        private async Task InvokeAuthorizeEndpoint()
        {
            var authorizeRequest = new AuthorizeRequest(_request.GetQuery());
            var clientIdContext = new OAuthValidateClientCredentialsContext(
                _request.Dictionary,
                authorizeRequest.ClientId,
                null,
                authorizeRequest.RedirectUri);

            await _options.Provider.ValidateClientCredentials(clientIdContext);
            if (clientIdContext.IsValidated)
            {
                authorizeRequest.RedirectUri = clientIdContext.RedirectUri;
                _authorizeRequest = authorizeRequest;
            }

            var authorizeEndpointContext = new OAuthAuthorizeEndpointContext(_request.Dictionary);
            await _options.Provider.AuthorizeEndpoint(authorizeEndpointContext);

            if (!authorizeEndpointContext.IsRequestCompleted)
            {
                await _next(_request.Dictionary);
            }
        }

        private async Task InvokeTokenEndpoint()
        {
            string text;
            using (var reader = new StreamReader(_request.Body))
            {
                text = await reader.ReadToEndAsync();
            }
            var form = new NameValueCollection();
            OwinHelpers.ParseDelimited(
                text,
                new[] { '&' },
                (name, value, state) => ((NameValueCollection)state).Add(name, value),
                form);

            AccessTokenRequest accessTokenRequest = AccessTokenRequest.Create(form.Get);
            var authorizationCodeAccessTokenRequest = accessTokenRequest as AuthorizationCodeAccessTokenRequest;
            var clientCredentialsAccessTokenRequest = accessTokenRequest as ClientCredentialsAccessTokenRequest;
            var resourceOwnerPasswordCredentialsAccessTokenRequest = accessTokenRequest as ResourceOwnerPasswordCredentialsAccessTokenRequest;

            OAuthValidateClientCredentialsContext lookupClientId = await AuthenticateClient(authorizationCodeAccessTokenRequest);

            if (!lookupClientId.IsValidated)
            {
                // TODO: actual error
                return;
            }

            ClaimsIdentity identity = null;
            IDictionary<string, string> extra = null;
            if (authorizationCodeAccessTokenRequest != null)
            {
                byte[] protectedData = Convert.FromBase64String(authorizationCodeAccessTokenRequest.Code.Replace('-', '+').Replace('_', '/'));
                byte[] userData = _options.DataProtection.Unprotect(protectedData);
                DataModel model = DataModelSerialization.Deserialize(userData);
                identity = model.Principal.Identity as ClaimsIdentity ?? new ClaimsIdentity(model.Principal.Identity);
                extra = model.Extra;
            }
            if (resourceOwnerPasswordCredentialsAccessTokenRequest != null)
            {
                var resourceOwnerCredentialsContext = new OAuthValidateResourceOwnerCredentialsContext(
                    _request.Dictionary,
                    resourceOwnerPasswordCredentialsAccessTokenRequest.Username,
                    resourceOwnerPasswordCredentialsAccessTokenRequest.Password,
                    resourceOwnerPasswordCredentialsAccessTokenRequest.Scope);

                _options.Provider.ValidateResourceOwnerCredentials(resourceOwnerCredentialsContext);

                if (resourceOwnerCredentialsContext.IsValidated)
                {
                    identity = resourceOwnerCredentialsContext.Identity;
                    extra = resourceOwnerCredentialsContext.Extra;
                }
                else
                {
                    throw new NotImplementedException("real error");
                }
            }

            var tokenEndpointContext = new OAuthTokenEndpointContext(
                _request.Dictionary,
                identity,
                extra ?? new Dictionary<string, string>(StringComparer.Ordinal),
                accessTokenRequest);

            await _options.Provider.TokenEndpoint(tokenEndpointContext);

            if (!tokenEndpointContext.TokenIssued)
            {
                throw new NotImplementedException("real error");
            }

            var model2 = new DataModel(new ClaimsPrincipal(tokenEndpointContext.Identity), tokenEndpointContext.Extra);
            byte[] userData2 = DataModelSerialization.Serialize(model2);
            byte[] protectedData2 = _options.DataProtection.Protect(userData2);
            string text2 = Convert.ToBase64String(protectedData2).Replace('+', '-').Replace('/', '_');

            var memory = new MemoryStream();
            byte[] body;
            using (var writer = new JsonTextWriter(new StreamWriter(memory)))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("access_token");
                writer.WriteValue(text2);
                writer.WritePropertyName("token_type");
                writer.WriteValue("bearer");
                writer.WritePropertyName("expires_in");
                writer.WriteValue(3600);
                writer.WriteEndObject();
                writer.Flush();
                body = memory.ToArray();
            }
            _response.ContentType = "application/json;charset=UTF-8";
            _response.SetHeader("Cache-Control", "no-store");
            _response.SetHeader("Pragma", "no-cache");
            _response.SetHeader("Content-Length", memory.ToArray().Length.ToString(CultureInfo.InvariantCulture));
            _response.Write(body);
        }

        private async Task<OAuthValidateClientCredentialsContext> AuthenticateClient(AuthorizationCodeAccessTokenRequest authorizationCodeAccessTokenRequest)
        {
            string clientId = null;
            string clientSecret = null;
            string redirectUri = null;

            if (authorizationCodeAccessTokenRequest != null)
            {
                clientId = authorizationCodeAccessTokenRequest.ClientId;
                redirectUri = authorizationCodeAccessTokenRequest.RedirectUri;
            }

            string authorization = _request.GetHeader("Authorization");
            if (!string.IsNullOrWhiteSpace(authorization) && authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                byte[] data = Convert.FromBase64String(authorization.Substring("Basic ".Length).Trim());
                string text = Encoding.UTF8.GetString(data);
                int delimiterIndex = text.IndexOf(':');
                if (delimiterIndex >= 0)
                {
                    string name = text.Substring(0, delimiterIndex);
                    string password = text.Substring(delimiterIndex + 1);

                    if (clientId != null && !string.Equals(clientId, name, StringComparison.Ordinal))
                    {
                        return null;
                    }

                    clientId = name;
                    clientSecret = password;
                }
            }

            var lookupClientIdContext = new OAuthValidateClientCredentialsContext(
                _request.Dictionary,
                clientId,
                clientSecret,
                redirectUri);

            await _options.Provider.ValidateClientCredentials(lookupClientIdContext);

            return lookupClientIdContext;
        }
    }
}
