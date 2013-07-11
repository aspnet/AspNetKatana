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
using System.Collections.Generic;
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
        private OAuthLookupClientContext _clientContext;

        public OAuthAuthorizationServerHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected override Task<AuthenticationTicket> AuthenticateCore()
        {
            return Task.FromResult<AuthenticationTicket>(null);
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

            var clientContext = await ValidateAuthorizeEndpointClientAsync(authorizeRequest);

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

            if (!authorizeRequest.IsAuthorizationCodeGrantType &&
                !authorizeRequest.IsImplicitGrantType)
            {
                _logger.WriteVerbose("Authorize endpoint request contains unsupported response_type parameter");
                await SendErrorRedirectAsync(clientContext, "unsupported_response_type");
                return true;
            }

            _clientContext = clientContext;
            _authorizeEndpointRequest = authorizeRequest;

            var authorizeEndpointContext = new OAuthAuthorizeEndpointContext(Context);

            await Options.Provider.AuthorizeEndpoint(authorizeEndpointContext);

            return authorizeEndpointContext.IsRequestCompleted;
        }


        protected override async Task ApplyResponseGrant()
        {
            // only successful results of an authorize request are altered
            if (_clientContext == null ||
                _authorizeEndpointRequest == null ||
                Response.StatusCode != 200)
            {
                return;
            }

            // only apply with signin of matching authentication type
            var signin = Helper.LookupSignIn(Options.AuthenticationType);
            if (signin == null)
            {
                return;
            }

            string location = _clientContext.EffectiveRedirectUri;

            if (_authorizeEndpointRequest.IsAuthorizationCodeGrantType)
            {
                DateTimeOffset currentUtc = Options.SystemClock.UtcNow;
                signin.Extra.IssuedUtc = currentUtc;
                signin.Extra.ExpiresUtc = currentUtc.Add(Options.AuthenticationCodeExpireTimeSpan);

                // associate client_id with all subsequent tickets
                signin.Extra.Properties["client_id"] = _authorizeEndpointRequest.ClientId;
                if (!string.IsNullOrEmpty(_authorizeEndpointRequest.RedirectUri))
                {
                    // keep original request parameter for later comparison
                    signin.Extra.Properties["redirect_uri"] = _authorizeEndpointRequest.RedirectUri;
                }

                var context = new AuthenticationTokenCreateContext(
                    Context,
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
            else if (_authorizeEndpointRequest.IsImplicitGrantType)
            {
                DateTimeOffset currentUtc = Options.SystemClock.UtcNow;
                signin.Extra.IssuedUtc = currentUtc;
                signin.Extra.ExpiresUtc = currentUtc.Add(Options.AccessTokenExpireTimeSpan);

                // associate client_id with access token
                signin.Extra.Properties["client_id"] = _authorizeEndpointRequest.ClientId;

                var accessTokenContext = new AuthenticationTokenCreateContext(
                    Context,
                    Options.AccessTokenFormat,
                    new AuthenticationTicket(signin.Identity, signin.Extra));

                await Options.AccessTokenProvider.CreateAsync(accessTokenContext);

                var accessToken = accessTokenContext.Token;
                if (string.IsNullOrEmpty(accessToken))
                {
                    accessToken = accessTokenContext.SerializeTicket();
                }

                DateTimeOffset? accessTokenExpiresUtc = accessTokenContext.Ticket.Extra.ExpiresUtc;
   
                var appender = new Appender(location, '#');
                appender
                    .Append("access_token", accessToken)
                    .Append("token_type=bearer");
                if (accessTokenExpiresUtc.HasValue)
                {
                    var expiresTimeSpan = accessTokenExpiresUtc - currentUtc;
                    var expiresIn = (long)(expiresTimeSpan.Value.TotalSeconds + .5);
                    appender.Append("expires_in", expiresIn.ToString(CultureInfo.InvariantCulture));
                }
                if (!String.IsNullOrEmpty(_authorizeEndpointRequest.State))
                {
                    appender.Append("state", _authorizeEndpointRequest.State);
                }
                Response.Redirect(appender.ToString());
            }
        }
        class Appender
        {
            private readonly char _delimiter;
            private readonly StringBuilder _sb;
            private bool _hasDelimiter;

            public Appender(string value, char delimiter)
            {
                _sb = new StringBuilder(value);
                _delimiter = delimiter;
                _hasDelimiter = value.IndexOf(delimiter) != -1;
            }

            public Appender Append(string encodedPair)
            {
                _sb.Append(_hasDelimiter ? '&' : _delimiter)
                   .Append(encodedPair);
                _hasDelimiter = true;
                return this;
            }

            public Appender Append(string name, string value)
            {
                _sb.Append(_hasDelimiter ? '&' : _delimiter)
                   .Append(Uri.EscapeDataString(name))
                   .Append('=')
                   .Append(Uri.EscapeDataString(value));
                _hasDelimiter = true;
                return this;
            }
            public override string ToString()
            {
                return _sb.ToString();
            }
        }

        private async Task InvokeTokenEndpoint()
        {
            _logger.WriteVerbose("InvokeTokenEndpoint");

            var form = await Request.ReadFormAsync();

            TokenEndpointRequest tokenEndpointRequest = new TokenEndpointRequest(form);

            OAuthLookupClientContext clientContext = await ValidateTokenEndpointClientAsync(tokenEndpointRequest);

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
                    Context,
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

                string clientId;
                if (!ticket.Extra.Properties.TryGetValue("client_id", out clientId) ||
                    !String.Equals(clientId, tokenEndpointRequest.ClientId, StringComparison.Ordinal))
                {
                    _logger.WriteError("authorization code does not contain matching client_id");
                    await SendErrorJsonAsync("invalid_grant");
                    return;
                }

                string redirectUri;
                if (ticket.Extra.Properties.TryGetValue("redirect_uri", out redirectUri))
                {
                    ticket.Extra.Properties.Remove("redirect_uri");
                    if (!String.Equals(redirectUri, tokenEndpointRequest.AuthorizationCode.RedirectUri, StringComparison.Ordinal))
                    {
                        _logger.WriteError("authorization code does not contain matching redirect_uri");
                        await SendErrorJsonAsync("invalid_grant");
                        return;
                    }
                }
            }
            else if (tokenEndpointRequest.IsRefreshTokenGrantType)
            {
                var refreshTokenContext = new AuthenticationTokenReceiveContext(
                    Context,
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

                if (!ticket.Extra.ExpiresUtc.HasValue ||
                    ticket.Extra.ExpiresUtc < currentUtc)
                {
                    _logger.WriteError("expired refresh token");
                    await SendErrorJsonAsync("invalid_grant");
                    return;
                }
            }
            else if (tokenEndpointRequest.IsResourceOwnerPasswordCredentialsGrantType)
            {
                var resourceOwnerCredentialsContext = new OAuthValidateResourceOwnerCredentialsContext(
                    Context,
                    clientContext.ClientId,
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
            else if (tokenEndpointRequest.IsClientCredentialsGrantType)
            {
                var clientCredentialsContext = new OAuthValidateClientCredentialsContext(
                    Context,
                    clientContext.ClientId,
                    tokenEndpointRequest.ClientCredentials.Scope);

                await Options.Provider.ValidateClientCredentials(clientCredentialsContext);

                if (clientCredentialsContext.IsValidated)
                {
                    ticket = new AuthenticationTicket(
                        clientCredentialsContext.Identity,
                        clientCredentialsContext.Extra);
                }
                else
                {
                    _logger.WriteError("client credentials grant is not valid.");
                    await SendErrorJsonAsync("unauthorized_client");
                    return;
                }
            }
            else
            {
                _logger.WriteError("grant type is not recognized");
                await SendErrorJsonAsync("unsupported_grant_type");
                return;
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
                Context,
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
                Context,
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
            OAuthLookupClientContext context,
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
                var location = WebUtilities.AddQueryString(context.EffectiveRedirectUri, "error", error);
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

        private async Task<OAuthLookupClientContext> ValidateAuthorizeEndpointClientAsync(AuthorizeEndpointRequest authorizeRequest)
        {
            var clientContext = new OAuthLookupClientContext(
                Context,
                new ClientDetails
                {
                    ClientId = authorizeRequest.ClientId,
                    RedirectUri = authorizeRequest.RedirectUri
                },
                isValidatingRedirectUri: true,
                isValidatingClientSecret: false);
            await Options.Provider.LookupClient(clientContext);
            return clientContext;
        }

        private async Task<OAuthLookupClientContext> ValidateTokenEndpointClientAsync(TokenEndpointRequest tokenRequest)
        {
            var clientDetails = new ClientDetails();

            string authorization = Request.Headers.Get("Authorization");
            if (!string.IsNullOrWhiteSpace(authorization) && authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                byte[] data = Convert.FromBase64String(authorization.Substring("Basic ".Length).Trim());
                string text = Encoding.UTF8.GetString(data);
                int delimiterIndex = text.IndexOf(':');
                if (delimiterIndex >= 0)
                {
                    clientDetails.ClientId = text.Substring(0, delimiterIndex);
                    clientDetails.ClientSecret = text.Substring(delimiterIndex + 1);
                }
            }

            if (!string.IsNullOrEmpty(tokenRequest.ClientId))
            {
                if (string.IsNullOrEmpty(clientDetails.ClientId))
                {
                    clientDetails.ClientId = tokenRequest.ClientId;
                }
                else if (!String.Equals(tokenRequest.ClientId, clientDetails.ClientId, StringComparison.Ordinal))
                {
                    // mismatched client id between authentication header and form parameter,
                    // return non-validated context
                    return new OAuthLookupClientContext(
                        Context,
                        clientDetails, isValidatingRedirectUri: false,
                        isValidatingClientSecret: false);
                }
            }

            var clientContext = new OAuthLookupClientContext(
                Context,
                clientDetails,
                isValidatingRedirectUri: false,
                isValidatingClientSecret: true);

            await Options.Provider.LookupClient(clientContext);

            return clientContext;
        }
    }
}

#endif
