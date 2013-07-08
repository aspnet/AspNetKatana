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

#if AUTHSERVER

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth.Messages;
using Newtonsoft.Json;

namespace Microsoft.Owin.Security.OAuth
{
    internal class OAuthAuthorizationServerHandler : AuthenticationHandler<OAuthAuthorizationServerOptions>
    {
        private readonly ILogger _logger;

        private AuthorizeEndpointRequest _authorizeEndpointRequest;

        public OAuthAuthorizationServerHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected override Task<AuthenticationTicket> AuthenticateCore()
        {
            return Task.FromResult<AuthenticationTicket>(null);
        }

        protected override async Task ApplyResponseGrant()
        {
            // only successful results of an authorize request are altered
            if (_authorizeEndpointRequest == null || Response.StatusCode != 200)
            {
                return;
            }

            // only apply with signin of matching authentication type
            var signin = Helper.LookupSignIn(Options.AuthenticationType);
            if (signin == null)
            {
                return;
            }

            string location = _authorizeEndpointRequest.RedirectUri;

            if (_authorizeEndpointRequest.ResponseType == "code")
            {
                DateTimeOffset currentUtc = Options.SystemClock.UtcNow;
                signin.Extra.IssuedUtc = currentUtc;
                signin.Extra.ExpiresUtc = currentUtc.Add(Options.AuthenticationCodeExpireTimeSpan);

                var context = new AuthenticationTokenCreateContext(
                    Options.AuthenticationCodeFormat,
                    new AuthenticationTicket(signin.Identity, signin.Extra));

                await Options.AuthenticationCodeProvider.CreateAsync(context);

                var code = context.Token;
                if (string.IsNullOrEmpty(code))
                {
                    code = context.SerializeTicket();
                }

                location = WebUtilities.AddQueryString(location, "code", code);
                if (!String.IsNullOrEmpty(_authorizeEndpointRequest.State))
                {
                    location = WebUtilities.AddQueryString(location, "state", _authorizeEndpointRequest.State);
                }
                Response.Redirect(location);
            }
            else if (_authorizeEndpointRequest.ResponseType == "token")
            {
                // todo - implicit grant flow
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
            var authorizeRequest = new AuthorizeEndpointRequest(Request.Query);

            var clientContext = await ValidateClientAsync(authorizeRequest);

            if (!clientContext.IsValidated)
            {
                _logger.WriteVerbose("Unable to validate client information");
                await SendErrorRedirectAsync(clientContext, "invalid_request");
                return true;
            }

            if (string.IsNullOrEmpty(authorizeRequest.ResponseType))
            {
                _logger.WriteVerbose("Authorize endpoint request missing required response_type parameter");
                await SendErrorRedirectAsync(clientContext, "invalid_request");
                return true;
            }

            if (!authorizeRequest.ResponseTypeIsCode && !authorizeRequest.ResponseTypeIsToken)
            {
                _logger.WriteVerbose("Authorize endpoint request contains unsupported response_type parameter");
                await SendErrorRedirectAsync(clientContext, "unsupported_response_type");
                return true;
            }

            authorizeRequest.RedirectUri = clientContext.RedirectUri;
            _authorizeEndpointRequest = authorizeRequest;

            var authorizeEndpointContext = new OAuthAuthorizeEndpointContext(Context);

            await Options.Provider.AuthorizeEndpoint(authorizeEndpointContext);

            return authorizeEndpointContext.IsRequestCompleted;
        }

        private async Task InvokeTokenEndpoint()
        {
            _logger.WriteVerbose("InvokeTokenEndpoint");

            var form = await Request.ReadFormAsync();

            TokenEndpointRequest tokenEndpointRequest = new TokenEndpointRequest(form);

            OAuthValidateClientCredentialsContext clientContext = await ValidateClientAsync(tokenEndpointRequest);

            if (!clientContext.IsValidated)
            {
                // TODO: actual error
                _logger.WriteError("clientID is not valid.");
                await SendErrorJsonAsync("invalid_client");
                return;
            }

            DateTimeOffset currentUtc = Options.SystemClock.UtcNow;
            // remove milliseconds in case they don't round-trip
            currentUtc = currentUtc.Subtract(TimeSpan.FromMilliseconds(currentUtc.Millisecond));

            AuthenticationTicket ticket;
            if (tokenEndpointRequest.IsAuthorizationCodeGrantType)
            {
                var authenticationCodeContext = new AuthenticationTokenReceiveContext(
                    Options.AuthenticationCodeFormat,
                    tokenEndpointRequest.AuthorizationCode.Code);

                await Options.AuthenticationCodeProvider.ReceiveAsync(authenticationCodeContext);

                ticket = authenticationCodeContext.Ticket;

                if (ticket == null)
                {
                    _logger.WriteError("invalid authorization code");
                    await SendErrorJsonAsync("invalid_grant");
                    return;
                }

                if (!ticket.Extra.ExpiresUtc.HasValue ||
                    ticket.Extra.ExpiresUtc < currentUtc)
                {
                    _logger.WriteError("expired authorization code");
                    await SendErrorJsonAsync("invalid_grant");
                    return;
                }
            }
            else if (tokenEndpointRequest.IsRefreshTokenGrantType)
            {
                var refreshTokenContext = new AuthenticationTokenReceiveContext(
                    Options.RefreshTokenFormat,
                    tokenEndpointRequest.RefreshToken.RefreshToken);

                await Options.RefreshTokenProvider.ReceiveAsync(refreshTokenContext);

                ticket = refreshTokenContext.Ticket;

                if (ticket == null)
                {
                    _logger.WriteError("invalid refresh token");
                    await SendErrorJsonAsync("invalid_grant");
                    return;
                }
            }
            else if (tokenEndpointRequest.IsResourceOwnerPasswordCredentialsGrantType)
            {
                var resourceOwnerCredentialsContext = new OAuthValidateResourceOwnerCredentialsContext(
                    Context,
                    tokenEndpointRequest.ResourceOwnerPasswordCredentials.UserName,
                    tokenEndpointRequest.ResourceOwnerPasswordCredentials.Password,
                    tokenEndpointRequest.ResourceOwnerPasswordCredentials.Scope);

                await Options.Provider.ValidateResourceOwnerCredentials(resourceOwnerCredentialsContext);

                if (resourceOwnerCredentialsContext.IsValidated)
                {
                    ticket = new AuthenticationTicket(
                        resourceOwnerCredentialsContext.Identity,
                        resourceOwnerCredentialsContext.Extra);
                }
                else
                {
                    _logger.WriteError("resourceOwnerCredentialsContext is not valid.");
                    throw new NotImplementedException("real error");
                }
            }
            else
            {
                _logger.WriteError("null TokenEndpointRequestAuthorizationCode and null resourceOwnerPasswordCredentialsTokenRequest");
                throw new NotImplementedException("real error");
            }

            ticket.Extra.IssuedUtc = currentUtc;
            ticket.Extra.ExpiresUtc = currentUtc.Add(Options.AccessTokenExpireTimeSpan);

            var tokenEndpointContext = new OAuthTokenEndpointContext(
                Context,
                ticket,
                tokenEndpointRequest);

            await Options.Provider.TokenEndpoint(tokenEndpointContext);

            if (!tokenEndpointContext.TokenIssued)
            {
                _logger.WriteError("Token was not issued to tokenEndpointContext");
                throw new NotImplementedException("real error");
            }

            var accessTokenContext = new AuthenticationTokenCreateContext(
                Options.AccessTokenFormat,
                new AuthenticationTicket(tokenEndpointContext.Identity, tokenEndpointContext.Extra));
            await Options.AccessTokenProvider.CreateAsync(accessTokenContext);

            string accessToken = accessTokenContext.Token;
            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = accessTokenContext.SerializeTicket();
            }
            DateTimeOffset? accessTokenExpiresUtc = tokenEndpointContext.Extra.ExpiresUtc;

            var refreshTokenCreateContext = new AuthenticationTokenCreateContext(
                Options.RefreshTokenFormat,
                accessTokenContext.Ticket);
            await Options.RefreshTokenProvider.CreateAsync(refreshTokenCreateContext);
            string refreshToken = refreshTokenCreateContext.Token;

            var memory = new MemoryStream();
            byte[] body;
            using (var writer = new JsonTextWriter(new StreamWriter(memory)))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("access_token");
                writer.WriteValue(accessToken);
                writer.WritePropertyName("token_type");
                writer.WriteValue("bearer");
                if (accessTokenExpiresUtc.HasValue)
                {
                    var expiresTimeSpan = accessTokenExpiresUtc - currentUtc;
                    long expiresIn = (long)(expiresTimeSpan.Value.TotalSeconds + .5);
                    if (expiresIn > 0)
                    {
                        writer.WritePropertyName("expires_in");
                        writer.WriteValue(expiresIn);
                    }
                }
                if (!String.IsNullOrEmpty(refreshToken))
                {
                    writer.WritePropertyName("refresh_token");
                    writer.WriteValue(refreshToken);
                }
                writer.WriteEndObject();
                writer.Flush();
                body = memory.ToArray();
            }
            Response.ContentType = "application/json;charset=UTF-8";
            Response.Headers.Set("Cache-Control", "no-cache");
            Response.Headers.Set("Pragma", "no-cache");
            Response.Headers.Set("Expires", "-1");
            Response.ContentLength = memory.ToArray().Length;
            await Response.WriteAsync(body, Response.CallCancelled);
        }

        private async Task SendErrorRedirectAsync(
            OAuthValidateClientCredentialsContext context,
            string error,
            string errorDescription = null,
            string errorUri = null)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (error == null)
            {
                throw new ArgumentNullException("error");
            }
            if (context.IsValidated)
            {
                // redirect with error if client_id and redirect_uri have been validated
                var location = WebUtilities.AddQueryString(context.RedirectUri, "error", error);
                if (!string.IsNullOrEmpty(errorDescription))
                {
                    location = WebUtilities.AddQueryString(location, "error_description", errorDescription);
                }
                if (!string.IsNullOrEmpty(errorDescription))
                {
                    location = WebUtilities.AddQueryString(location, "error_uri", errorUri);
                }
                Response.Redirect(location);
            }
            else
            {
                // write error in response body if client_id or redirect_uri have not been validated
                await SendErrorPageAsync(error, errorDescription, errorUri);
            }
        }

        private async Task SendErrorJsonAsync(string error, string errorDescription = null, string errorUri = null)
        {
            var memory = new MemoryStream();
            byte[] body;
            using (var writer = new JsonTextWriter(new StreamWriter(memory)))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("error");
                writer.WriteValue(error);
                if (!string.IsNullOrEmpty(errorDescription))
                {
                    writer.WritePropertyName("error_description");
                    writer.WriteValue(errorDescription);
                }
                if (!string.IsNullOrEmpty(errorUri))
                {
                    writer.WritePropertyName("error_uri");
                    writer.WriteValue(errorUri);
                }
                writer.WriteEndObject();
                writer.Flush();
                body = memory.ToArray();
            }
            Response.StatusCode = 400;
            Response.ContentType = "application/json;charset=UTF-8";
            Response.Headers.Set("Cache-Control", "no-cache");
            Response.Headers.Set("Pragma", "no-cache");
            Response.Headers.Set("Expires", "-1");
            Response.Headers.Set("Content-Length", body.Length.ToString(CultureInfo.InvariantCulture));
            await Response.Body.WriteAsync(body, 0, body.Length);
        }

        private async Task SendErrorPageAsync(string error, string errorDescription, string errorUri)
        {
            var memory = new MemoryStream();
            byte[] body;
            using (var writer = new StreamWriter(memory))
            {
                writer.WriteLine("error: {0}", error);
                if (!string.IsNullOrEmpty(errorDescription))
                {
                    writer.WriteLine("error_description: {0}", error);
                }
                if (!string.IsNullOrEmpty(errorUri))
                {
                    writer.WriteLine("error_uri: {0}", error);
                }
                writer.Flush();
                body = memory.ToArray();
            }
            Response.StatusCode = 400;
            Response.ContentType = "text/plain;charset=UTF-8";
            Response.Headers.Set("Cache-Control", "no-cache");
            Response.Headers.Set("Pragma", "no-cache");
            Response.Headers.Set("Expires", "-1");
            Response.Headers.Set("Content-Length", body.Length.ToString(CultureInfo.InvariantCulture));
            await Response.Body.WriteAsync(body, 0, body.Length);
        }

        private async Task<OAuthValidateClientCredentialsContext> ValidateClientAsync(AuthorizeEndpointRequest authorizeRequest)
        {
            var clientContext = new OAuthValidateClientCredentialsContext(
                Context,
                authorizeRequest.ClientId,
                null,
                authorizeRequest.RedirectUri);

            return await ValidateClientAsync(clientContext);
        }

        private async Task<OAuthValidateClientCredentialsContext> ValidateClientAsync(TokenEndpointRequest tokenEndpointRequest)
        {
            string clientId = null;
            string clientSecret = null;
            string redirectUri = null;

            if (tokenEndpointRequest.IsAuthorizationCodeGrantType)
            {
                clientId = tokenEndpointRequest.AuthorizationCode.ClientId;
                redirectUri = tokenEndpointRequest.AuthorizationCode.RedirectUri;
            }

            string authorization = Request.Headers.Get("Authorization");
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
                        // return a context that is not validated
                        return new OAuthValidateClientCredentialsContext(
                            Context,
                            clientId,
                            null,
                            redirectUri);
                    }

                    clientId = name;
                    clientSecret = password;
                }
            }

            var clientContext = new OAuthValidateClientCredentialsContext(
                Context,
                clientId,
                clientSecret,
                redirectUri);

            return await ValidateClientAsync(clientContext);
        }

        private async Task<OAuthValidateClientCredentialsContext> ValidateClientAsync(OAuthValidateClientCredentialsContext clientContext)
        {
            await Options.Provider.ValidateClientCredentials(clientContext);
            return clientContext;
        }
    }
}

#endif
