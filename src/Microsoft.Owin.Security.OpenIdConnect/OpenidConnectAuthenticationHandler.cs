// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.OpenIdConnect
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Runtime.ExceptionServices;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Extensions;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.Owin.Logging;
    using Microsoft.Owin.Security.DataHandler.Encoder;
    using Microsoft.Owin.Security.Infrastructure;
    using Microsoft.Owin.Security.Notifications;

    /// <summary>
    /// OWIN handler for OpenIdConnect
    /// </summary>
    public class OpenIdConnectAuthenticationHandler : AuthenticationHandler<OpenIdConnectAuthenticationOptions>
    {
        private const string HandledResponse = "HandledResponse";

        private static readonly RNGCryptoServiceProvider Random = new RNGCryptoServiceProvider();
        private static char base64PadCharacter = '=';
        private static char base64Character62 = '+';
        private static char base64Character63 = '/';
        private static char base64UrlCharacter62 = '-';
        private static char base64UrlCharacter63 = '_';
        private readonly ILogger _logger;

        public OpenIdConnectAuthenticationHandler(ILogger logger)
        {
            _logger = logger;
        }

        private string CurrentUri
        {
            get
            {
                return Request.Scheme +
                       Uri.SchemeDelimiter +
                       Request.Host +
                       Request.PathBase +
                       Request.Path +
                       Request.QueryString;
            }
        }

        /// <summary>
        /// Handles Signout
        /// </summary>
        /// <returns></returns>
        protected override async Task ApplyResponseGrantAsync()
        {
            AuthenticationResponseRevoke signout = Helper.LookupSignOut(Options.AuthenticationType, Options.AuthenticationMode);
            if (signout != null)
            {
                OpenIdConnectMessage openIdConnectMessage = new OpenIdConnectMessage()
                {
                    IssuerAddress = Options.EndSessionEndpoint ?? string.Empty,
                    RequestType = OpenIdConnectRequestType.LogoutRequest,
                };

                // Set End_Session_Endpoint in order:
                // 1. properties.Redirect
                // 2. Options.Wreply
                AuthenticationProperties properties = signout.Properties;
                if (properties != null && !string.IsNullOrEmpty(properties.RedirectUri))
                {
                    openIdConnectMessage.PostLogoutRedirectUri = properties.RedirectUri;
                }
                else if (!string.IsNullOrWhiteSpace(Options.PostLogoutRedirectUri))
                {
                    openIdConnectMessage.PostLogoutRedirectUri = Options.PostLogoutRedirectUri;
                }

                if (Options.Notifications != null && Options.Notifications.RedirectToIdentityProvider != null)
                {
                    var notification = new RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>(Context, Options)
                    {
                        ProtocolMessage = openIdConnectMessage
                    };
                    await Options.Notifications.RedirectToIdentityProvider(notification);
                    if (notification.Skipped)
                    {
                        return;
                    }
                }

                string redirect = openIdConnectMessage.CreateLogoutUrl();
                if (!Uri.IsWellFormedUriString(redirect, UriKind.Absolute))
                {
                    _logger.WriteError(string.Format(CultureInfo.InvariantCulture, Resources.Exception_RedirectUri_LogoutQueryString_IsNotWellFormed, redirect));
                    return;
                }

                Response.Redirect(redirect);
            }
        }

        /// <summary>
        /// Handles SignIn
        /// </summary>
        /// <returns></returns>
        protected override async Task ApplyResponseChallengeAsync()
        {
            if (Response.StatusCode == 401)
            {
                AuthenticationResponseChallenge challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);
                if (challenge == null)
                {
                    return;
                }

                // order for redirect_uri
                // 1. challenge.Properties.RedirectUri
                // 2. CurrentUri
                AuthenticationProperties properties = challenge.Properties;
                if (string.IsNullOrEmpty(properties.RedirectUri))
                {
                    properties.RedirectUri = CurrentUri;
                }

                // this value will be passed to the AuthorizationCodeReceivedNotification
                if (!string.IsNullOrWhiteSpace(Options.RedirectUri))
                {
                    properties.Dictionary.Add(OpenIdConnectAuthenticationDefaults.RedirectUriUsedForCodeKey, Options.RedirectUri);
                }

                // TODO - introduce delegate for creating messages
                OpenIdConnectMessage openIdConnectMessage = new OpenIdConnectMessage
                {
                    ClientId = Options.ClientId,
                    IssuerAddress = Options.AuthorizationEndpoint ?? string.Empty,
                    Nonce = GenerateNonce(),
                    RedirectUri = Options.RedirectUri,
                    RequestType = OpenIdConnectRequestType.AuthenticationRequest,
                    Resource = Options.Resource,
                    ResponseMode = Options.ResponseMode,
                    ResponseType = Options.ResponseType,
                    Scope = Options.Scope,
                    State = OpenIdConnectAuthenticationDefaults.AuthenticationPropertiesKey + "=" + Uri.EscapeDataString(Options.StateDataFormat.Protect(properties)),
                };

                if (Options.Notifications != null && Options.Notifications.RedirectToIdentityProvider != null)
                {
                    var notification = new RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>(Context, Options)
                    {
                        ProtocolMessage = openIdConnectMessage
                    };

                    await Options.Notifications.RedirectToIdentityProvider(notification);
                    if (notification.Skipped)
                    {
                        _logger.WriteInformation("Options.Notifications.RedirectToIdentityProvider skipped.");
                        return;
                    }
                }

                string redirect = openIdConnectMessage.CreateIdTokenUrl();
                _logger.WriteInformation("OpenIdConnectRequest, redirecting to: " + redirect);
                if (!Uri.IsWellFormedUriString(redirect, UriKind.Absolute))
                {
                    _logger.WriteError(string.Format(CultureInfo.InvariantCulture, "The OpenIdConnectRequest sign-in redirect uri is not well formed: '{0}'", redirect));
                    return;
                }

                Response.Redirect(redirect);
            }

            return;
        }

        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            // Allow login to be constrained to a specific path. Need to make this runtime configurable.
            if (Options.AuthorizeCallback.HasValue && Options.AuthorizeCallback != (Request.PathBase + Request.Path))
            {
                return null;
            }

            OpenIdConnectMessage openIdConnectMessage = null;

            // assumption: if the ContentType is "application/x-www-form-urlencoded" it should be safe to read as it is small.
            if (string.Equals(Request.Method, "POST", StringComparison.OrdinalIgnoreCase)
              && !string.IsNullOrWhiteSpace(Request.ContentType)
                // May have media/type; charset=utf-8, allow partial match.
              && Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase)
              && Request.Body.CanRead)
            {
                if (!Request.Body.CanSeek)
                {
                    // Buffer in case this body was not meant for us.
                    MemoryStream memoryStream = new MemoryStream();
                    await Request.Body.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    Request.Body = memoryStream;
                }

                IFormCollection form = await Request.ReadFormAsync();
                Request.Body.Seek(0, SeekOrigin.Begin);

                // Post preview release: a delegate on OpenIdConnectAuthenticationOptions would allow for users to hook their own custom message.
                openIdConnectMessage = new OpenIdConnectMessage(form);
            }

            if (openIdConnectMessage == null)
            {
                return null;
            }

            ExceptionDispatchInfo authFailedEx = null;
            try
            {
                MessageReceivedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> messageReceivedNotification = null;
                if (Options.Notifications != null && Options.Notifications.MessageReceived != null)
                {
                    messageReceivedNotification = new MessageReceivedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>(Context, Options)
                    {
                        ProtocolMessage = openIdConnectMessage
                    };

                    await Options.Notifications.MessageReceived(messageReceivedNotification);
                    if (messageReceivedNotification.HandledResponse)
                    {
                        return GetHandledResponseTicket();
                    }

                    if (messageReceivedNotification.Skipped)
                    {
                        return null;
                    }
                }

                if (!string.IsNullOrWhiteSpace(openIdConnectMessage.Error))
                {
                    throw new OpenIdConnectProtocolException(
                         "OpenIdConnectMessage.Error was not null, indicating a possible error: '" + openIdConnectMessage.Error
                       + "' Error_Description (may be empty): '" + openIdConnectMessage.ErrorDescription ?? string.Empty
                       + "' Error_Uri (may be empty): '" + openIdConnectMessage.ErrorUri ?? string.Empty + ".'");
                }

                // code is only accepted with id_token, in this version, hence check for code is inside this if
                // OpenIdConnect protocol allows a Code to be received without the id_token
                if (openIdConnectMessage.IdToken != null)
                {
                    if (Options.Notifications != null && Options.Notifications.SecurityTokenReceived != null && !string.IsNullOrWhiteSpace(openIdConnectMessage.IdToken))
                    {
                        var securityTokenReceivedNotification = new SecurityTokenReceivedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>(Context, Options)
                        {
                            ProtocolMessage = openIdConnectMessage,
                        };

                        await Options.Notifications.SecurityTokenReceived(securityTokenReceivedNotification);
                        if (securityTokenReceivedNotification.HandledResponse)
                        {
                            return GetHandledResponseTicket();
                        }

                        if (securityTokenReceivedNotification.Skipped)
                        {
                            return null;
                        }
                    }

                    // TODO could be cleaner
                    ClaimsPrincipal principal = Options.SecurityTokenHandlers.ValidateToken(openIdConnectMessage.IdToken, Options.TokenValidationParameters);
                    ClaimsIdentity claimsIdentity = principal.Identity as ClaimsIdentity;

                    // claims principal could have changed claim values, use bits received on wire for validation.
                    JwtSecurityToken jwt = new JwtSecurityToken(openIdConnectMessage.IdToken);
                    ValidateNonce(jwt, _logger);
                    AuthenticationTicket ticket = new AuthenticationTicket(claimsIdentity, GetPropertiesFromState(openIdConnectMessage.State));
                    if (Options.Notifications != null && Options.Notifications.SecurityTokenValidated != null)
                    {
                        var securityTokenValidatedNotification = new SecurityTokenValidatedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>(Context, Options)
                        {
                            AuthenticationTicket = ticket,
                            ProtocolMessage = openIdConnectMessage,
                        };

                        await Options.Notifications.SecurityTokenValidated(securityTokenValidatedNotification);
                        if (securityTokenValidatedNotification.HandledResponse)
                        {
                            return GetHandledResponseTicket();
                        }

                        if (securityTokenValidatedNotification.Skipped)
                        {
                            return null;
                        }

                        // Flow possible changes
                        ticket = securityTokenValidatedNotification.AuthenticationTicket;
                    }

                    if (openIdConnectMessage.Code != null)
                    {
                        ValidateCHash(openIdConnectMessage.Code, jwt, _logger);
                        if (Options.Notifications != null && Options.Notifications.AuthorizationCodeReceived != null)
                        {
                            var authorizationCodeReceivedNotification = new AuthorizationCodeReceivedNotification(Context, Options)
                            {
                                AuthenticationTicket = ticket,
                                Code = openIdConnectMessage.Code,
                                JwtSecurityToken = jwt,
                                ProtocolMessage = openIdConnectMessage,
                                Redirect_Uri = ticket.Properties.Dictionary.ContainsKey(OpenIdConnectAuthenticationDefaults.RedirectUriUsedForCodeKey) ?
                                    ticket.Properties.Dictionary[OpenIdConnectAuthenticationDefaults.RedirectUriUsedForCodeKey] : string.Empty,
                            };

                            await Options.Notifications.AuthorizationCodeReceived(authorizationCodeReceivedNotification);
                            if (authorizationCodeReceivedNotification.HandledResponse)
                            {
                                return GetHandledResponseTicket();
                            }
                            if (authorizationCodeReceivedNotification.Skipped)
                            {
                                return null;
                            }

                            // Flow possible changes
                            ticket = authorizationCodeReceivedNotification.AuthenticationTicket;
                        }
                    }

                    return ticket;
                }
            }
            catch (Exception exception)
            {
                // We can't await inside a catch block, capture and handle outside.
                authFailedEx = ExceptionDispatchInfo.Capture(exception);
            }
            finally
            {
                DeleteNonce(Response, Options.AuthenticationType);
            }

            if (authFailedEx != null)
            {
                _logger.WriteError("Exception occurred while processing message: '" + authFailedEx.ToString());

                 if (Options.Notifications != null && Options.Notifications.AuthenticationFailed != null)
                 {
                    // Post preview release: user can update metadata, need consistent messaging.
                    var authenticationFailedNotification = new AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>(Context, Options)
                    {
                        ProtocolMessage = openIdConnectMessage,
                        Exception = authFailedEx.SourceException
                    };

                    await Options.Notifications.AuthenticationFailed(authenticationFailedNotification);
                    if (authenticationFailedNotification.HandledResponse)
                    {
                        return GetHandledResponseTicket();
                    }

                    if (authenticationFailedNotification.Skipped)
                    {
                        return null;
                    }
                }

                authFailedEx.Throw();
            }

            return null;
        }

        protected string GenerateNonce()
        {
            string nonceKey = OpenIdConnectAuthenticationDefaults.CookiePrefix + OpenIdConnectAuthenticationDefaults.Nonce + Options.AuthenticationType;

            var nonceBytes = new byte[32];
            Random.GetBytes(nonceBytes);
            string nonceId = TextEncodings.Base64Url.Encode(nonceBytes) + TextEncodings.Base64Url.Encode(Guid.NewGuid().ToByteArray());

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsSecure
            };

            Response.Cookies.Append(nonceKey, nonceId, cookieOptions);
            return nonceId;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "Microsoft.Owin.Logging.LoggerExtensions.WriteWarning(Microsoft.Owin.Logging.ILogger,System.String,System.String[])",
        Justification = "Logging is not Localized")]
        private static void ValidateCHash(string code, JwtSecurityToken jwt, ILogger logger)
        {
            // validate the Hash(oir.Code) == jwt.CodeClaim
            // When a response_type is id_token + code, the code must == a special hash of a claim inside the token.
            // Its value is the base64url encoding of the left-most half of the hash of the octets of the ASCII representation of the 'code'. 
            // where the hash algorithm used is the hash algorithm used in the alg parameter of the ID Token's JWS 
            // For instance, if the alg is RS256, hash the access_token value with SHA-256, then take the left-most 128 bits and base64url encode them.

            HashAlgorithm hashAlgorithm = null;
            if (!jwt.Payload.ContainsKey(JwtConstants.ReservedClaims.CHash))
            {
                string message = string.Format(CultureInfo.InvariantCulture, Resources.ProtocolException_CHashClaimNotFoundInJwt, JwtConstants.ReservedClaims.CHash, jwt.RawData ?? string.Empty);
                if (logger != null)
                {
                    logger.WriteError(message);
                }

                throw new OpenIdConnectProtocolException(message);
            }

            string c_hashInToken = jwt.Payload[JwtConstants.ReservedClaims.CHash] as string;
            if (c_hashInToken == null)
            {                
                string message = string.Format(CultureInfo.InvariantCulture, Resources.ProtocolException_CHashClaimInJwtPayloadIsNotAString, jwt.RawData ?? string.Empty);
                if (logger != null)
                {
                    logger.WriteError(message);
                }

                throw new OpenIdConnectProtocolException(message);
            }

            if (string.IsNullOrEmpty(c_hashInToken))
            {                
                string message = string.Format(CultureInfo.InvariantCulture, Resources.ProtocolException_CHashClaimInJwtPayloadIsNullOrEmpty, jwt.RawData ?? string.Empty);
                if (logger != null)
                {
                    logger.WriteError(message);
                }

                throw new OpenIdConnectProtocolException(message);
            }

            string algorithm = string.Empty;
            if (!jwt.Header.TryGetValue(JwtConstants.ReservedHeaderParameters.Alg, out algorithm))
            {
                algorithm = JwtConstants.Algorithms.RSA_SHA256;
            }

            algorithm = GetHashAlgorithm(algorithm);
            try
            {
                try
                {
                    hashAlgorithm = HashAlgorithm.Create(algorithm);
                }
                catch (Exception ex)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, Resources.ProtocolException_UnableToCreateHashAlgorithmWhenValidatingCHash, algorithm, jwt.RawData ?? string.Empty);
                    if (logger != null)
                    {
                        logger.WriteError(message);
                    }

                    throw new OpenIdConnectProtocolException(message, ex);
                }

                if (hashAlgorithm == null)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, Resources.ProtocolException_UnableToCreateNullHashAlgorithmWhenValidatingCHash, algorithm, jwt.RawData ?? string.Empty);
                    if (logger != null)
                    {
                        logger.WriteError(message);
                    }

                    throw new OpenIdConnectProtocolException(message);
                }

                byte[] hashBytes = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(code));
                string hashString = Convert.ToBase64String(hashBytes, 0, hashBytes.Length / 2);
                hashString = hashString.Split(base64PadCharacter)[0]; // Remove any trailing padding
                hashString = hashString.Replace(base64Character62, base64UrlCharacter62); // 62nd char of encoding
                hashString = hashString.Replace(base64Character63, base64UrlCharacter63); // 63rd char of encoding

                if (!StringComparer.Ordinal.Equals(c_hashInToken, hashString))
                {
                    string message = string.Format(CultureInfo.InvariantCulture, Resources.ProtocolException_CHashNotValid, c_hashInToken, code, algorithm, jwt.RawData ?? string.Empty);
                    if (logger != null)
                    {
                        logger.WriteError(message);
                    }

                    throw new OpenIdConnectProtocolException(message);
                }
            }
            finally
            {
                if (hashAlgorithm != null)
                {
                    hashAlgorithm.Dispose();
                }
            }
        }

        private static string GetHashAlgorithm(string algorithm)
        {
            switch (algorithm)
            {
                case JwtConstants.Algorithms.ECDSA_SHA256:
                case JwtConstants.Algorithms.RSA_SHA256:
                case JwtConstants.Algorithms.HMAC_SHA256:
                    return "SHA256";

                case JwtConstants.Algorithms.ECDSA_SHA384:
                case JwtConstants.Algorithms.RSA_SHA384:
                case JwtConstants.Algorithms.HMAC_SHA384:
                    return "SHA384";

                case JwtConstants.Algorithms.ECDSA_SHA512:
                case JwtConstants.Algorithms.RSA_SHA512:
                case JwtConstants.Algorithms.HMAC_SHA512:
                    return "SHA512";

                default:
                    return "SHA256";
          }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "Microsoft.Owin.Logging.LoggerExtensions.WriteWarning(Microsoft.Owin.Logging.ILogger,System.String,System.String[])",
            Justification = "Logging is not Localized")]
        private static void DeleteNonce(IOwinResponse response, string context)
        {
            string nonceKey = OpenIdConnectAuthenticationDefaults.CookiePrefix + OpenIdConnectAuthenticationDefaults.Nonce + context;
            response.Cookies.Delete(nonceKey);
        }

        private static string GetCookieNonce(IOwinRequest request, string context)
        {
            string nonceKey = OpenIdConnectAuthenticationDefaults.CookiePrefix + OpenIdConnectAuthenticationDefaults.Nonce + context;
            return request.Cookies[nonceKey];
        }

        private AuthenticationProperties GetPropertiesFromState(string state)
        {
            // assume a well formed query string: <a=b&>OpenIdConnectAuthenticationDefaults.AuthenticationPropertiesKey=kasjd;fljasldkjflksdj<&c=d>
            int startIndex = 0;
            if (string.IsNullOrWhiteSpace(state) || (startIndex = state.IndexOf(OpenIdConnectAuthenticationDefaults.AuthenticationPropertiesKey, StringComparison.Ordinal)) == -1)
            {
                return new AuthenticationProperties();
            }

            int authenticationIndex = startIndex + OpenIdConnectAuthenticationDefaults.AuthenticationPropertiesKey.Length;
            if (authenticationIndex == -1 || authenticationIndex == state.Length || state[authenticationIndex] != '=')
            {
                return new AuthenticationProperties();
            }

            // scan rest of string looking for '&'
            authenticationIndex++;
            int endIndex = state.Substring(authenticationIndex, state.Length - authenticationIndex).IndexOf("&", StringComparison.Ordinal);

            // -1 => no other parameters are after the AuthenticationPropertiesKey
            if (endIndex == -1)
            {
                return Options.StateDataFormat.Unprotect(Uri.UnescapeDataString(state.Substring(authenticationIndex).Replace('+', ' ')));
            }
            else
            {
                return Options.StateDataFormat.Unprotect(Uri.UnescapeDataString(state.Substring(authenticationIndex, endIndex).Replace('+', ' ')));
            }
        }

        /// <summary>
        /// Calls InvokeReplyPathAsync
        /// </summary>
        /// <returns>True if the request was handled, false if the next middleware should be invoked.</returns>
        public override Task<bool> InvokeAsync()
        {
            return InvokeReplyPathAsync();
        }

        private async Task<bool> InvokeReplyPathAsync()
        {
            AuthenticationTicket ticket = await AuthenticateAsync();

            if (ticket != null)
            {
                string value;
                if (ticket.Properties.Dictionary.TryGetValue(HandledResponse, out value) && value == "true")
                {
                    return true;
                }
                if (ticket.Identity != null)
                {
                    Request.Context.Authentication.SignIn(ticket.Properties, ticket.Identity);
                }
                // Redirect back to the original secured resource, if any.
                if (!string.IsNullOrWhiteSpace(ticket.Properties.RedirectUri))
                {
                    Response.Redirect(ticket.Properties.RedirectUri);
                    return true;
                }
            }

            return false;
        }

        private static AuthenticationTicket GetHandledResponseTicket()
        {
            return new AuthenticationTicket(null, new AuthenticationProperties(new Dictionary<string, string>() { { HandledResponse, "true" } }));
        }

        private void ValidateNonce(JwtSecurityToken jwt, ILogger logger)
        {
            string nonceFoundInJwt = jwt.Payload.Nonce;
            if (nonceFoundInJwt == null || string.IsNullOrWhiteSpace(nonceFoundInJwt))
            {
                string message = string.Format(CultureInfo.InvariantCulture, Resources.ProtocolException_NonceClaimNotFoundInJwt, JwtConstants.ReservedClaims.Nonce, jwt.RawData ?? string.Empty);
                if (logger != null)
                {
                    logger.WriteError(message);
                }

                throw new OpenIdConnectProtocolException(message);
            }

            // add delegate so users can add nonce
            // could link nonce through state.
            string expectedNonce = GetCookieNonce(Request, Options.AuthenticationType);
            if (string.IsNullOrWhiteSpace(expectedNonce))
            {
                string message = string.Format(CultureInfo.InvariantCulture, Resources.ProtocolException_NonceWasNotFound, nonceFoundInJwt);
                if (logger != null)
                {
                    logger.WriteError(message);
                }

                throw new OpenIdConnectProtocolException(message);
            }

            if (!(StringComparer.Ordinal.Equals(nonceFoundInJwt, expectedNonce)))
            {
                string message = string.Format(CultureInfo.InvariantCulture, Resources.ProtocolException_NonceInJwtDoesNotMatchExpected, nonceFoundInJwt, expectedNonce);
                if (logger != null)
                {
                    logger.WriteError(message);
                }

                throw new OpenIdConnectProtocolException(message);
            }

            return;
        }
    }
}
