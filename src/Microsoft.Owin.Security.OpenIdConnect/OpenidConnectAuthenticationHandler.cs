// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.OpenIdConnect
{
    using System;
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

        /// <summary>
        /// Handles Signout
        /// </summary>
        /// <returns></returns>
        protected override async Task ApplyResponseGrantAsync()
        {
            AuthenticationResponseRevoke signout = Helper.LookupSignOut(Options.AuthenticationType, Options.AuthenticationMode);
            if (signout != null)
            {
                // TODO - introduce delegate for creating messages
                OpenIdConnectMessage openIdConnectMessage = new OpenIdConnectMessage()
                {
                    LogoutEndpoint = Options.EndSessionEndpoint ?? string.Empty,
                };

                if (Options.Notifications != null && Options.Notifications.RedirectToIdentityProvider != null)
                {
                    RedirectToIdentityProviderNotification<OpenIdConnectMessage> notification =
                        new RedirectToIdentityProviderNotification<OpenIdConnectMessage> { ProtocolMessage = openIdConnectMessage };
                    await Options.Notifications.RedirectToIdentityProvider(notification);
                    if (notification.Cancel)
                    {
                        return;
                    }
                }

                string redirect = openIdConnectMessage.CreateLogoutQueryString();
                if (!Uri.IsWellFormedUriString(redirect, UriKind.Absolute))
                {
                    _logger.WriteError(string.Format(CultureInfo.InvariantCulture, Resources.Exception_RedirectUri_LogoutQueryString_IsNotWellFormed, redirect));
                    return;
                }

                Response.Redirect(redirect);
            }
        }

        protected override async Task ApplyResponseChallengeAsync()
        {
            if (Response.StatusCode == 401)
            {
                AuthenticationResponseChallenge challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);
                if (challenge == null)
                {
                    return;
                }

                string baseUri =
                        Request.Scheme +
                        Uri.SchemeDelimiter +
                        Request.Host +
                        Request.PathBase;

                string currentUri =
                    baseUri +
                    Request.Path +
                    Request.QueryString;

                string nonce = Guid.NewGuid().ToString();

                AuthenticationProperties properties = challenge.Properties;
                if (string.IsNullOrEmpty(properties.RedirectUri))
                {
                    properties.RedirectUri = currentUri;
                }              

                // TODO - introduce delegate for creating messages
                OpenIdConnectMessage openIdConnectMessage = new OpenIdConnectMessage
                {
                    Client_Id = Options.Client_Id,
                    IssuerAddress = Options.AuthorizeEndpoint ?? string.Empty,
                    Nonce = GenerateNonce(),
                    Redirect_Uri = Options.Redirect_Uri,
                    Response_Mode = Options.Response_Mode,
                    Response_Type = OpenIdConnectResponseTypes.Code_Id_Token,
                    Scope = Options.Scope,
                };

                if (Options.Notifications != null && Options.Notifications.RedirectToIdentityProvider != null)
                {
                    RedirectToIdentityProviderNotification<OpenIdConnectMessage> notification = new RedirectToIdentityProviderNotification<OpenIdConnectMessage>
                    {
                        ProtocolMessage = openIdConnectMessage
                    };

                    await Options.Notifications.RedirectToIdentityProvider(notification);
                    if (notification.Cancel)
                    {
                        return;
                    }
                }

                string redirect = openIdConnectMessage.CreateIdTokenQueryString();
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
            // Allow login to be constrained to a specific path.
            if (Options.AuthorizeCallback.HasValue && Options.AuthorizeCallback != Request.Path)
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

                // Post preview release: a delegate on OpenIdConnectAuthenticationOptions would allow for users to hook their own custom message.
                openIdConnectMessage = new OpenIdConnectMessage(form);
            }

            if (openIdConnectMessage == null)
            {
                Request.Body.Seek(0, SeekOrigin.Begin);
                return null;
            }

            // if an error shows up, we can't be sure it's ours, log it, reset body and return
            if (openIdConnectMessage.Error != null)
            {
                _logger.WriteError(typeof(OpenIdConnectAuthenticationHandler).ToString()
                    + ".Error was not null, indicating a possible error: '" + openIdConnectMessage.Error
                    + "' Error_Description (may be empty): '" + openIdConnectMessage.Error_Description ?? string.Empty
                    + "' Error_Uri (may be empty): '" + openIdConnectMessage.Error_Uri ?? string.Empty + ".'");
                Request.Body.Seek(0, SeekOrigin.Begin);
                return null;
            }

            if (openIdConnectMessage.Id_Token != null)
            {
                MessageReceivedNotification<OpenIdConnectMessage> messageReceivedNotification = null;
                if (Options.Notifications != null && Options.Notifications.MessageReceived != null)
                {
                    messageReceivedNotification = new MessageReceivedNotification<OpenIdConnectMessage> { ProtocolMessage = openIdConnectMessage };
                    await Options.Notifications.MessageReceived(messageReceivedNotification);
                }

                if (messageReceivedNotification != null && messageReceivedNotification.Cancel)
                {
                    return null;
                }

                if (Options.Notifications != null && Options.Notifications.SecurityTokenReceived != null)
                {
                    SecurityTokenReceivedNotification securityTokenReceivedNotification = new SecurityTokenReceivedNotification { SecurityToken = openIdConnectMessage.Id_Token };
                    await Options.Notifications.SecurityTokenReceived(securityTokenReceivedNotification);
                    if (securityTokenReceivedNotification.Cancel)
                    {
                        return null;
                    }
                }

                ExceptionDispatchInfo authFailedEx = null;
                try
                {
                    ClaimsPrincipal principal = Options.SecurityTokenHandlers.ValidateToken(openIdConnectMessage.Id_Token, Options.TokenValidationParameters);

                    // claims principal could have changed claim values, use bits received on wire for validation.
                    JwtSecurityToken jwt = new JwtSecurityToken(openIdConnectMessage.Id_Token);
                    ValidateNonce(jwt, _logger);
                    if (openIdConnectMessage.Code != null)
                    {
                        ValidateCHash(openIdConnectMessage.Code, jwt, _logger);
                        if (Options.Notifications != null && Options.Notifications.AccessCodeReceived != null)
                        {
                            await Options.Notifications.AccessCodeReceived(new AccessCodeReceivedNotification { AccessCode = openIdConnectMessage.Code, ProtocolMessage = openIdConnectMessage });
                        }
                    }

                    AuthenticationTicket ticket = new AuthenticationTicket(principal.Identity as ClaimsIdentity, new AuthenticationProperties());
                    ticket.Properties.Dictionary.Add(OpenIdConnectAuthenticationDefaults.CodeKey, openIdConnectMessage.Code);

                    // SignIn takes a collection of identities, but the Ticket has a place for only one, we add the first identity only.
                    Request.Context.Authentication.SignIn(ticket.Properties, ticket.Identity);
                    if (Options.Notifications != null && Options.Notifications.SecurityTokenValidated != null)
                    {
                        await Options.Notifications.SecurityTokenValidated(new SecurityTokenValidatedNotification { AuthenticationTicket = ticket });
                    }

                    return ticket;
                }
                catch (Exception exception)
                {
                    // We can't await inside a catch block, capture and handle outside.
                    authFailedEx = ExceptionDispatchInfo.Capture(exception);
                }

                if (authFailedEx != null)
                {
                    if (Options.Notifications != null && Options.Notifications.AuthenticationFailed != null)
                    {
                        // Post preview release: user can update metadata, need consistent messaging.
                        var authenticationFailedNotification = new AuthenticationFailedNotification<OpenIdConnectMessage>()
                        {
                            ProtocolMessage = openIdConnectMessage,
                            Exception = authFailedEx.SourceException
                        };

                        await Options.Notifications.AuthenticationFailed(authenticationFailedNotification);
                        if (!authenticationFailedNotification.Cancel)
                        {
                            authFailedEx.Throw();
                        }
                    }
                    else
                    {
                        authFailedEx.Throw();
                    }
                }
            }

            return null;
        }

        protected string GenerateNonce()
        {
            string nonceKey = OpenIdConnectAuthenticationDefaults.CookiePrefix + OpenIdConnectAuthenticationDefaults.Nonce + Options.AuthenticationType;

            var nonceBytes = new byte[32];
            Random.GetBytes(nonceBytes);
            string nonceId = TextEncodings.Base64Url.Encode(nonceBytes);

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
            if (!jwt.Payload.ContainsKey(JwtConstants.ReservedClaims.C_Hash))
            {
                string message = string.Format(CultureInfo.InvariantCulture, Resources.ProtocolException_CHashClaimNotFoundInJwt, JwtConstants.ReservedClaims.C_Hash, jwt.RawData ?? string.Empty);
                if (logger != null)
                {
                    logger.WriteError(message);
                }

                throw new OpenIdConnectProtocolException(message);
            }

            string c_hashInToken = jwt.Payload[JwtConstants.ReservedClaims.C_Hash] as string;
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
            if (!jwt.Header.TryGetValue(JwtConstants.ReservedHeaderParameters.Algorithm, out algorithm))
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

            DeleteNonce(Response, Options.AuthenticationType);
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

        private static string GetCookieNonce(IOwinRequest request, string context)
        {
            string nonceKey = OpenIdConnectAuthenticationDefaults.CookiePrefix + OpenIdConnectAuthenticationDefaults.Nonce + context;
            return request.Cookies[nonceKey];
        }

        private static void DeleteNonce(IOwinResponse response, string context)
        {
            string nonceKey = OpenIdConnectAuthenticationDefaults.CookiePrefix + OpenIdConnectAuthenticationDefaults.Nonce + context;
            response.Cookies.Delete(nonceKey);
        }
    }
}
