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
using System.Globalization;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth.Messages;
using Newtonsoft.Json;

namespace Microsoft.Owin.Security.OAuth
{
    internal class OAuthAuthorizationServerHandler : AuthenticationHandler<OAuthAuthorizationServerOptions>
    {
        private readonly IProtectionHandler<AuthenticationData> _modelProtectionHandler;

        private AuthorizeRequest _authorizeRequest;

        public OAuthAuthorizationServerHandler(IProtectionHandler<AuthenticationData> modelProtectionHandler)
        {
            _modelProtectionHandler = modelProtectionHandler;
        }

        protected override async Task<AuthenticationData> AuthenticateCore()
        {
            return null;
        }

        protected override async Task ApplyResponseGrant()
        {
            var signin = Helper.LookupSignin("Bearer");
            if (_authorizeRequest != null && signin != null)
            {
                var model = new AuthenticationData(signin.Identity, signin.Extra ?? new Dictionary<string, string>(StringComparer.Ordinal));
                var text = _modelProtectionHandler.ProtectModel(model);

                string redirectUrl = _authorizeRequest.RedirectUri;
                bool hasQueryString = redirectUrl.IndexOf('?') != -1;

                if (_authorizeRequest.ResponseType == "code")
                {
                    redirectUrl +=
                        (hasQueryString ? "&code=" : "?code=") +
                            Uri.EscapeDataString(text) +
                            "&state=" +
                            Uri.EscapeDataString(_authorizeRequest.State);
                    Response.Redirect(redirectUrl);
                }
                else if (_authorizeRequest.ResponseType == "token")
                {
                }
            }
        }

        public override async Task<bool> Invoke()
        {
            if (!string.IsNullOrEmpty(Options.AuthorizeEndpointPath) && string.Equals(Options.AuthorizeEndpointPath, Request.Path, StringComparison.OrdinalIgnoreCase))
            {
                return await InvokeAuthorizeEndpoint();
            }
            if (!string.IsNullOrEmpty(Options.TokenEndpointPath) && string.Equals(Options.TokenEndpointPath, Request.Path, StringComparison.OrdinalIgnoreCase))
            {
                await InvokeTokenEndpoint();
                return true;
            }
            return false;
        }

        private async Task<bool> InvokeAuthorizeEndpoint()
        {
            var authorizeRequest = new AuthorizeRequest(Request.GetQuery());
            var clientIdContext = new OAuthValidateClientCredentialsContext(
                Request.Environment,
                authorizeRequest.ClientId,
                null,
                authorizeRequest.RedirectUri);

            await Options.Provider.ValidateClientCredentials(clientIdContext);
            if (clientIdContext.IsValidated)
            {
                authorizeRequest.RedirectUri = clientIdContext.RedirectUri;
                _authorizeRequest = authorizeRequest;
            }

            var authorizeEndpointContext = new OAuthAuthorizeEndpointContext(Request.Environment);
            await Options.Provider.AuthorizeEndpoint(authorizeEndpointContext);

            return authorizeEndpointContext.IsRequestCompleted;
        }

        private async Task InvokeTokenEndpoint()
        {
            string text;
            using (var reader = new StreamReader(Request.Body))
            {
                text = await reader.ReadToEndAsync();
            }

            var form = await Request.ReadForm();

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
                var model = _modelProtectionHandler.UnprotectModel(authorizationCodeAccessTokenRequest.Code);
                identity = model.Identity;
                extra = model.Extra;
            }

            if (resourceOwnerPasswordCredentialsAccessTokenRequest != null)
            {
                var resourceOwnerCredentialsContext = new OAuthValidateResourceOwnerCredentialsContext(
                    Request.Environment,
                    resourceOwnerPasswordCredentialsAccessTokenRequest.Username,
                    resourceOwnerPasswordCredentialsAccessTokenRequest.Password,
                    resourceOwnerPasswordCredentialsAccessTokenRequest.Scope);

                Options.Provider.ValidateResourceOwnerCredentials(resourceOwnerCredentialsContext);

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
                Request.Environment,
                identity,
                extra ?? new Dictionary<string, string>(StringComparer.Ordinal),
                accessTokenRequest);

            await Options.Provider.TokenEndpoint(tokenEndpointContext);

            if (!tokenEndpointContext.TokenIssued)
            {
                throw new NotImplementedException("real error");
            }

            string text2 = _modelProtectionHandler.ProtectModel(new AuthenticationData(tokenEndpointContext.Identity, tokenEndpointContext.Extra));

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
            Response.ContentType = "application/json;charset=UTF-8";
            Response.SetHeader("Cache-Control", "no-store");
            Response.SetHeader("Pragma", "no-cache");
            Response.SetHeader("Content-Length", memory.ToArray().Length.ToString(CultureInfo.InvariantCulture));
            await Response.Body.WriteAsync(body, 0, body.Length);
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

            string authorization = Request.GetHeader("Authorization");
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
                Request.Environment,
                clientId,
                clientSecret,
                redirectUri);

            await Options.Provider.ValidateClientCredentials(lookupClientIdContext);

            return lookupClientIdContext;
        }
    }
}
