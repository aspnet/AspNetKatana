// <copyright file="TwitterAuthenticationContext.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.Twitter.Messages;

namespace Microsoft.Owin.Security.Twitter
{
    internal class TwitterAuthenticationHandler : AuthenticationHandler<TwitterAuthenticationOptions>
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        private const string StateCookie = "__TwitterState";

        private const string RequestTokenEndpoint = "https://api.twitter.com/oauth/request_token";

        private const string AuthenticationEndpoint = "https://twitter.com/oauth/authenticate?oauth_token=";

        private const string AccessTokenEndpoint = "https://api.twitter.com/oauth/access_token";

        private readonly ILogger _logger;

        private readonly ISecureDataHandler<RequestToken> _tokenProtectionHandler;

        public TwitterAuthenticationHandler(ILogger logger, ISecureDataHandler<RequestToken> tokenProtectionHandler)
        {
            _logger = logger;
            this._tokenProtectionHandler = tokenProtectionHandler;
        }

        public override async Task<bool> Invoke()
        {
            if (Options.CallbackUrlPath != null &&
                String.Equals(Options.CallbackUrlPath, Request.Path, StringComparison.OrdinalIgnoreCase))
            {
                return await InvokeReturnPath();
            }
            return false;
        }

        protected override async Task<AuthenticationTicket> AuthenticateCore()
        {
            _logger.WriteVerbose("AuthenticateCore");

            try
            {
                IDictionary<string, string[]> query = Request.GetQuery();
                var protectedRequestToken = Request.GetCookies()[StateCookie];

                var requestToken = _tokenProtectionHandler.Unprotect(protectedRequestToken);

                if (requestToken == null)
                {
                    _logger.WriteWarning("Invalid state", null);
                    return null;
                }

                if (!query.ContainsKey("oauth_token"))
                {
                    _logger.WriteWarning("Missing oauth_token", null);
                    return null;
                }

                if (!query.ContainsKey("oauth_verifier"))
                {
                    _logger.WriteWarning("Missing oauth_verifier", null);
                    return null;
                }

                var returnedToken = query["oauth_token"].FirstOrDefault();
                string oauthVerifier = query["oauth_verifier"].FirstOrDefault();

                if (returnedToken != requestToken.Token)
                {
                    _logger.WriteWarning("Unmatched token", null);
                    return null;
                }

                if (string.IsNullOrWhiteSpace(oauthVerifier))
                {
                    _logger.WriteWarning("Blank oauth_verifier", null);
                    return null;
                }

                var accessToken = await ObtainAccessToken(Options.ConsumerKey, Options.ConsumerSecret, requestToken, oauthVerifier);

                var context = new TwitterAuthenticatedContext(Request.Environment, accessToken.UserId, accessToken.ScreenName);
                context.Identity = new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, accessToken.UserId, "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType), 
                        new Claim(ClaimTypes.Name, accessToken.ScreenName, "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType), 
                        new Claim("urn:twitter:userid", accessToken.UserId, "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType), 
                        new Claim("urn:twitter:screenname", accessToken.ScreenName, "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType),
                    },
                    Options.AuthenticationType,
                    ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
                context.Extra = requestToken.Extra;

                Response.DeleteCookie(StateCookie);

                await Options.Provider.Authenticated(context);

                return new AuthenticationTicket(context.Identity, context.Extra);
            }
            catch (Exception ex)
            {
                _logger.WriteError("Authentication failed", ex);
                return null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "MemoryStream.Dispose is idempotent")]
        protected override async Task ApplyResponseChallenge()
        {
            _logger.WriteVerbose("ApplyResponseChallenge");

            if (Response.StatusCode != 401)
            {
                return;
            }

            var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

            if (challenge != null)
            {
                string requestPrefix = Request.Scheme + "://" + Request.Host;
                string callBackUrl = requestPrefix + RequestPathBase + Options.CallbackUrlPath;

                var extra = challenge.Extra;
                if (string.IsNullOrEmpty(extra.RedirectUrl))
                {
                    extra.RedirectUrl = WebUtils.AddQueryString(requestPrefix + Request.PathBase + Request.Path, Request.QueryString);
                }

                var requestToken = await ObtainRequestToken(Options.ConsumerKey, Options.ConsumerSecret, callBackUrl, extra);

                if (requestToken.CallbackConfirmed)
                {
                    string twitterAuthenticationEndpoint = AuthenticationEndpoint + requestToken.Token;

                    var cookieOptions = new CookieOptions { HttpOnly = true };
                    if (Request.Scheme.ToUpperInvariant() == "HTTPS")
                    {
                        cookieOptions.Secure = true;
                    }

                    Response.StatusCode = 302;
                    Response.AddCookie(StateCookie, _tokenProtectionHandler.Protect(requestToken), cookieOptions);
                    Response.SetHeader("Location", twitterAuthenticationEndpoint);
                }

                // TODO: Error here.
            }

            // TODO: Find a way to move errors up the stack.
        }

        public async Task<bool> InvokeReturnPath()
        {
            _logger.WriteVerbose("InvokeReturnPath");

            var model = await Authenticate();

            var context = new TwitterReturnEndpointContext(Request.Environment, model)
                {
                    SignInAsAuthenticationType = this.Options.SignInAsAuthenticationType,
                    RedirectUri = model.Extra.RedirectUrl
                };
            model.Extra.RedirectUrl = null;

            await Options.Provider.ReturnEndpoint(context);

            if (context.SignInAsAuthenticationType != null && context.Identity != null)
            {
                ClaimsIdentity signInIdentity = context.Identity;
                if (!string.Equals(signInIdentity.AuthenticationType, context.SignInAsAuthenticationType, StringComparison.Ordinal))
                {
                    signInIdentity = new ClaimsIdentity(signInIdentity.Claims, context.SignInAsAuthenticationType, signInIdentity.NameClaimType, signInIdentity.RoleClaimType);
                }
                Response.Grant(signInIdentity, context.Extra);
            }

            if (!context.IsRequestCompleted && context.RedirectUri != null)
            {
                Response.Redirect(context.RedirectUri);
                context.RequestCompleted();
            }

            return context.IsRequestCompleted;
        }

        private HttpWebRequest CreateTwitterWebRequest(string endpoint)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(endpoint);
            httpWebRequest.Method = "POST";
            httpWebRequest.ProtocolVersion = HttpVersion.Version11;
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Accept = "*/*";
            httpWebRequest.UserAgent = "katana twitter middleware";
            httpWebRequest.Timeout = Options.TwitterRequestTimeout;
            httpWebRequest.ServicePoint.Expect100Continue = false;
            return httpWebRequest;
        }

        private async Task<RequestToken> ObtainRequestToken(string consumerKey, string consumerSecret, string callBackUri, AuthenticationExtra extra)
        {
            _logger.WriteVerbose("ObtainRequestToken");

            var obtainRequestTokenRequest = CreateTwitterWebRequest(RequestTokenEndpoint);

            var nonce = Guid.NewGuid().ToString("N");

            var authorizationParts = new SortedDictionary<string, string>
            {
                { "oauth_callback", callBackUri }, 
                { "oauth_consumer_key", consumerKey }, 
                { "oauth_nonce", nonce }, 
                { "oauth_signature_method", "HMAC-SHA1" }, 
                { "oauth_timestamp", GenerateTimeStamp() }, 
                { "oauth_version", "1.0" }
            };

            var parameterBuilder = new StringBuilder();
            foreach (var authorizationKey in authorizationParts)
            {
                parameterBuilder.AppendFormat("{0}={1}&", Uri.EscapeDataString(authorizationKey.Key), Uri.EscapeDataString(authorizationKey.Value));
            }
            parameterBuilder.Length--;
            var parameterString = parameterBuilder.ToString();

            var canonicalizedRequestBuilder = new StringBuilder();
            canonicalizedRequestBuilder.Append(obtainRequestTokenRequest.Method);
            canonicalizedRequestBuilder.Append("&");
            canonicalizedRequestBuilder.Append(Uri.EscapeDataString(obtainRequestTokenRequest.RequestUri.ToString()));
            canonicalizedRequestBuilder.Append("&");
            canonicalizedRequestBuilder.Append(Uri.EscapeDataString(parameterString));

            var signature = ComputeSignature(consumerSecret, null, canonicalizedRequestBuilder.ToString());
            authorizationParts.Add("oauth_signature", signature);

            var authorizationHeaderBuilder = new StringBuilder();
            authorizationHeaderBuilder.Append("OAuth ");
            foreach (var authorizationPart in authorizationParts)
            {
                authorizationHeaderBuilder.AppendFormat(
                    "{0}=\"{1}\", ", authorizationPart.Key, Uri.EscapeDataString(authorizationPart.Value));
            }
            authorizationHeaderBuilder.Length = authorizationHeaderBuilder.Length - 2;

            obtainRequestTokenRequest.Headers.Add("Authorization", authorizationHeaderBuilder.ToString());

            // TODO : Error handling
            var obtainRequestTokenResponse = await obtainRequestTokenRequest.GetResponseAsync() as HttpWebResponse;
            using (var reader = new StreamReader(obtainRequestTokenResponse.GetResponseStream()))
            {
                string responseText = await reader.ReadToEndAsync();
                responseText = responseText.Replace('+', ' ');
                var responseParameters = responseText.Split('&').Select(responseParameter => responseParameter.Split('=')).ToDictionary(brokenParameter => brokenParameter[0], brokenParameter => brokenParameter[1]);

                if (responseParameters.ContainsKey("oauth_callback_confirmed") ||
                    string.Equals(responseParameters["oauth_callback_confirmed"], "true", StringComparison.InvariantCulture))
                {
                    return new RequestToken { Token = Uri.UnescapeDataString(responseParameters["oauth_token"]), TokenSecret = Uri.UnescapeDataString(responseParameters["oauth_token_secret"]), CallbackConfirmed = true, Extra = extra };
                }
            }

            return new RequestToken();
        }

        private async Task<AccessToken> ObtainAccessToken(string consumerKey, string consumerSecret, RequestToken token, string verifier)
        {
            _logger.WriteVerbose("ObtainAccessToken");

            var obtainAccessTokenRequest = CreateTwitterWebRequest(AccessTokenEndpoint);

            var nonce = Guid.NewGuid().ToString("N");

            var authorizationParts = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", consumerKey }, 
                { "oauth_nonce", nonce }, 
                { "oauth_signature_method", "HMAC-SHA1" }, 
                { "oauth_token", token.Token }, 
                { "oauth_timestamp", GenerateTimeStamp() },
                { "oauth_verifier", verifier },
                { "oauth_version", "1.0" },
            };

            var parameterBuilder = new StringBuilder();
            foreach (var authorizationKey in authorizationParts)
            {
                parameterBuilder.AppendFormat("{0}={1}&", Uri.EscapeDataString(authorizationKey.Key), Uri.EscapeDataString(authorizationKey.Value));
            }
            parameterBuilder.Length--;
            var parameterString = parameterBuilder.ToString();

            var canonicalizedRequestBuilder = new StringBuilder();
            canonicalizedRequestBuilder.Append(obtainAccessTokenRequest.Method);
            canonicalizedRequestBuilder.Append("&");
            canonicalizedRequestBuilder.Append(Uri.EscapeDataString(obtainAccessTokenRequest.RequestUri.ToString()));
            canonicalizedRequestBuilder.Append("&");
            canonicalizedRequestBuilder.Append(Uri.EscapeDataString(parameterString));

            var signature = ComputeSignature(consumerSecret, token.TokenSecret, canonicalizedRequestBuilder.ToString());
            authorizationParts.Add("oauth_signature", signature);

            authorizationParts.Remove("oauth_verifier");

            var authorizationHeaderBuilder = new StringBuilder();
            authorizationHeaderBuilder.Append("OAuth ");
            foreach (var authorizationPart in authorizationParts)
            {
                authorizationHeaderBuilder.AppendFormat(
                    "{0}=\"{1}\", ", authorizationPart.Key, Uri.EscapeDataString(authorizationPart.Value));
            }
            authorizationHeaderBuilder.Length = authorizationHeaderBuilder.Length - 2;

            obtainAccessTokenRequest.Headers.Add("Authorization", authorizationHeaderBuilder.ToString());

            var bodyData = "oauth_verifier=" + Uri.EscapeDataString(verifier);
            obtainAccessTokenRequest.ContentLength = bodyData.Length;
            using (var bodyStream = new StreamWriter(obtainAccessTokenRequest.GetRequestStream()))
            {
                bodyStream.Write(bodyData);
            }

            // TODO : Error handling
            try
            {
                var obtainAccessTokenResponse = await obtainAccessTokenRequest.GetResponseAsync() as HttpWebResponse;
                string responseText;
                using (var reader = new StreamReader(obtainAccessTokenResponse.GetResponseStream()))
                {
                    responseText = await reader.ReadToEndAsync();
                    responseText = responseText.Replace('+', ' ');
                }
                var responseParameters = responseText.Split('&').Select(responseParameter => responseParameter.Split('=')).ToDictionary(brokenParameter => brokenParameter[0], brokenParameter => brokenParameter[1]);

                return new AccessToken
                {
                    Token = Uri.UnescapeDataString(responseParameters["oauth_token"]),
                    TokenSecret = Uri.UnescapeDataString(responseParameters["oauth_token_secret"]),
                    UserId = Uri.UnescapeDataString(responseParameters["user_id"]),
                    ScreenName = Uri.UnescapeDataString(responseParameters["screen_name"])
                };
            }
            catch (WebException ex)
            {
                using (WebResponse response = ex.Response)
                {
                    var httpResponse = (HttpWebResponse)response;
                    System.Diagnostics.Debug.WriteLine(httpResponse.StatusCode);
                    using (Stream responseStream = response.GetResponseStream())
                    using (var reader = new StreamReader(responseStream))
                    {
                        string text = reader.ReadToEnd();
                        System.Diagnostics.Debug.WriteLine(text);
                    }
                }

                throw;
            }
            return null;
        }

        private static string GenerateTimeStamp()
        {
            var secondsSinceUnixEpocStart = DateTime.UtcNow - Epoch;
            return Convert.ToInt64(secondsSinceUnixEpocStart.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        private static string ComputeSignature(string consumerSecret, string tokenSecret, string signatureData)
        {
            var hash = new HMACSHA1
            {
                Key = Encoding.ASCII.GetBytes(
                    string.Format(
                        "{0}&{1}",
                        Uri.EscapeDataString(consumerSecret),
                        string.IsNullOrEmpty(tokenSecret) ? string.Empty : Uri.EscapeDataString(tokenSecret)))
            }.ComputeHash(Encoding.ASCII.GetBytes(signatureData));
            return Convert.ToBase64String(hash);
        }
    }
}
