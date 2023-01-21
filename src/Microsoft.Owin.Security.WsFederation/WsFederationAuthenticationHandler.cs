﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.Notifications;

namespace Microsoft.Owin.Security.WsFederation
{
    /// <summary>
    /// A per-request authentication handler for the WsFederationAuthenticationMiddleware.
    /// </summary>
    public class WsFederationAuthenticationHandler : AuthenticationHandler<WsFederationAuthenticationOptions>
    {
        private const string HandledResponse = "HandledResponse";

        private readonly ILogger _logger;
        private WsFederationConfiguration _configuration;

        /// <summary>
        /// Creates a new WsFederationAuthenticationHandler
        /// </summary>
        /// <param name="logger"></param>
        public WsFederationAuthenticationHandler(ILogger logger)
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
            if (signout == null)
            {
                return;
            }

            if (_configuration == null)
            {
                _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.Request.CallCancelled);
            }

            WsFederationMessage wsFederationMessage = new WsFederationMessage()
            {
                IssuerAddress = _configuration.TokenEndpoint ?? string.Empty,
                Wtrealm = Options.Wtrealm,
                Wa = WsFederationConstants.WsFederationActions.SignOut,
            };

            // Set Wreply in order:
            // 1. properties.Redirect
            // 2. Options.SignOutWreply
            // 3. Options.Wreply
            AuthenticationProperties properties = signout.Properties;
            if (properties != null && !string.IsNullOrEmpty(properties.RedirectUri))
            {
                wsFederationMessage.Wreply = properties.RedirectUri;
            }
            else if (!string.IsNullOrWhiteSpace(Options.SignOutWreply))
            {
                wsFederationMessage.Wreply = Options.SignOutWreply;
            }
            else if (!string.IsNullOrWhiteSpace(Options.Wreply))
            {
                wsFederationMessage.Wreply = Options.Wreply;
            }

            var notification = new RedirectToIdentityProviderNotification<WsFederationMessage, WsFederationAuthenticationOptions>(Context, Options)
            {
                ProtocolMessage = wsFederationMessage
            };
            await Options.Notifications.RedirectToIdentityProvider(notification);

            if (!notification.HandledResponse)
            {
                string redirectUri = notification.ProtocolMessage.CreateSignOutUrl();
                if (!Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
                {
                    _logger.WriteWarning("The sign-out redirect URI is malformed: " + redirectUri);
                }
                Response.Redirect(redirectUri);
            }
        }

        /// <summary>
        /// Handles Challenge
        /// </summary>
        /// <returns></returns>
        protected override async Task ApplyResponseChallengeAsync()
        {
            if (Response.StatusCode != 401)
            {
                return;
            }

            AuthenticationResponseChallenge challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);
            if (challenge == null)
            {
                return;
            }

            if (_configuration == null)
            {
                _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.Request.CallCancelled);
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

            // Save the original challenge URI so we can redirect back to it when we're done.
            AuthenticationProperties properties = challenge.Properties;
            if (string.IsNullOrEmpty(properties.RedirectUri))
            {
                properties.RedirectUri = currentUri;
            }

            WsFederationMessage wsFederationMessage = new WsFederationMessage()
            {
                IssuerAddress = _configuration.TokenEndpoint ?? string.Empty,
                Wtrealm = Options.Wtrealm,
                Wctx = WsFederationAuthenticationDefaults.WctxKey + "=" + Uri.EscapeDataString(Options.StateDataFormat.Protect(properties)),
                Wa = WsFederationConstants.WsFederationActions.SignIn,
            };

            if (!string.IsNullOrWhiteSpace(Options.Wreply))
            {
                wsFederationMessage.Wreply = Options.Wreply;
            }

            var notification = new RedirectToIdentityProviderNotification<WsFederationMessage, WsFederationAuthenticationOptions>(Context, Options)
            {
                ProtocolMessage = wsFederationMessage
            };
            await Options.Notifications.RedirectToIdentityProvider(notification);

            if (!notification.HandledResponse)
            {
                string redirectUri = notification.ProtocolMessage.CreateSignInUrl();
                if (!Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
                {
                    _logger.WriteWarning("The sign-in redirect URI is malformed: " + redirectUri);
                }
                Response.Redirect(redirectUri);
            }
        }

        /// <summary>
        /// Invoked to detect and process incoming authentication requests.
        /// </summary>
        /// <returns></returns>
        public override Task<bool> InvokeAsync()
        {
            return InvokeReplyPathAsync();
        }

        // Returns true if the request was handled, false if the next middleware should be invoked.
        private async Task<bool> InvokeReplyPathAsync()
        {
            AuthenticationTicket ticket = await AuthenticateAsync();
            if (ticket == null)
            {
                return false;
            }

            string value;
            if (ticket.Properties.Dictionary.TryGetValue(HandledResponse, out value) && value == "true")
            {
                return true;
            }
            if (ticket.Identity != null)
            {
                Request.Context.Authentication.SignIn(ticket.Properties, ticket.Identity);
            }
            var preRedirectNotification = new PreRedirectReceivedNotification<WsFederationMessage, WsFederationAuthenticationOptions>(Context, Options)
            {
                AuthenticationTicket = ticket
            };
            // Allows to hook custom redirection.
            // Helpful in case the request is a post request where the values get lost on redirecting to Identity provider.
            // This hook allows you to rehydrate the post request and handle it in the application.            
            await Options.Notifications.PreRedirectReceived(preRedirectNotification);

            if (ticket.Properties.Dictionary.TryGetValue("HandledRedirect", out value) && value == "true")
            {
                // Assumes that the redirection has been handled in the application and hence stops default redirection.
                return true;
            }
            // Redirect back to the original secured resource, if any.
            if (!string.IsNullOrWhiteSpace(ticket.Properties.RedirectUri))
            {
                Response.Redirect(ticket.Properties.RedirectUri);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Invoked to process incoming authentication messages.
        /// </summary>
        /// <returns></returns>
        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            // Allow login to be constrained to a specific path.
            if (Options.CallbackPath.HasValue && Options.CallbackPath != (Request.PathBase + Request.Path))
            {
                return null;
            }

            WsFederationMessage wsFederationMessage = null;

            // assumption: if the ContentType is "application/x-www-form-urlencoded" it should be safe to read as it is small.
            if (string.Equals(Request.Method, "POST", StringComparison.OrdinalIgnoreCase)
              && !string.IsNullOrWhiteSpace(Request.ContentType)
              // May have media/type; charset=utf-8, allow partial match.
              && Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase)
              && Request.Body.CanRead)
            {
                if (!Request.Body.CanSeek)
                {
                    _logger.WriteVerbose("Buffering request body");
                    // Buffer in case this body was not meant for us.
                    MemoryStream memoryStream = new MemoryStream();
                    await Request.Body.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    Request.Body = memoryStream;
                }
                IFormCollection form = await Request.ReadFormAsync();
                Request.Body.Seek(0, SeekOrigin.Begin);
    
                // TODO: a delegate on WsFederationAuthenticationOptions would allow for users to hook their own custom message.
                wsFederationMessage = new WsFederationMessage(form);
            }

            if (wsFederationMessage == null || !wsFederationMessage.IsSignInMessage)
            {
                return null;
            }

            ExceptionDispatchInfo authFailedEx = null;
            try
            {
                var messageReceivedNotification = new MessageReceivedNotification<WsFederationMessage, WsFederationAuthenticationOptions>(Context, Options)
                {
                    ProtocolMessage = wsFederationMessage
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

                if (wsFederationMessage.Wresult == null)
                {
                    _logger.WriteWarning("Received a sign-in message without a WResult.");
                    return null;
                }

                string token = wsFederationMessage.GetToken();
                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.WriteWarning("Received a sign-in message without a token.");
                    return null;
                }

                var securityTokenReceivedNotification = new SecurityTokenReceivedNotification<WsFederationMessage, WsFederationAuthenticationOptions>(Context, Options)
                {
                    ProtocolMessage = wsFederationMessage
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

                if (_configuration == null)
                {
                    _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.Request.CallCancelled);
                }

                // Copy and augment to avoid cross request race conditions for updated configurations.
                TokenValidationParameters tvp = Options.TokenValidationParameters.Clone();
                IEnumerable<string> issuers = new[] { _configuration.Issuer };
                tvp.ValidIssuers = (tvp.ValidIssuers == null ? issuers : tvp.ValidIssuers.Concat(issuers));
                tvp.IssuerSigningKeys = (tvp.IssuerSigningKeys == null ? _configuration.SigningKeys : tvp.IssuerSigningKeys.Concat(_configuration.SigningKeys));

                ClaimsPrincipal principal = null;
                SecurityToken parsedToken = null;
                foreach (var validator in Options.SecurityTokenHandlers)
                {
                    if (validator.CanReadToken(token))
                    {
                        principal = validator.ValidateToken(token, tvp, out parsedToken);
                        break;
                    }
                }

                if (principal == null)
                {
                    throw new SecurityTokenException("no validator found");
                }

                ClaimsIdentity claimsIdentity = principal.Identity as ClaimsIdentity;

                // Retrieve our cached redirect uri
                string state = wsFederationMessage.Wctx;
                // WsFed allows for uninitiated logins, state may be missing.
                AuthenticationProperties properties = GetPropertiesFromWctx(state);
                AuthenticationTicket ticket = new AuthenticationTicket(claimsIdentity, properties);

                if (Options.UseTokenLifetime)
                {
                    // Override any session persistence to match the token lifetime.
                    DateTime issued = parsedToken.ValidFrom;
                    if (issued != DateTime.MinValue)
                    {
                        ticket.Properties.IssuedUtc = issued.ToUniversalTime();
                    }
                    DateTime expires = parsedToken.ValidTo;
                    if (expires != DateTime.MinValue)
                    {
                        ticket.Properties.ExpiresUtc = expires.ToUniversalTime();
                    }
                    ticket.Properties.AllowRefresh = false;
                }

                var securityTokenValidatedNotification = new SecurityTokenValidatedNotification<WsFederationMessage, WsFederationAuthenticationOptions>(Context, Options)
                {
                    AuthenticationTicket = ticket,
                    ProtocolMessage = wsFederationMessage,
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

                return ticket;
            }
            catch (Exception exception)
            {
                // We can't await inside a catch block, capture and handle outside.
                authFailedEx = ExceptionDispatchInfo.Capture(exception);
            }

            if (authFailedEx != null)
            {
                _logger.WriteError("Exception occurred while processing message: ", authFailedEx.SourceException);

                // Refresh the configuration for exceptions that may be caused by key rollovers. The user can also request a refresh in the notification.
                if (Options.RefreshOnIssuerKeyNotFound && authFailedEx.SourceException.GetType().Equals(typeof(SecurityTokenSignatureKeyNotFoundException)))
                {
                    Options.ConfigurationManager.RequestRefresh();
                }

                var authenticationFailedNotification = new AuthenticationFailedNotification<WsFederationMessage, WsFederationAuthenticationOptions>(Context, Options)
                {
                    ProtocolMessage = wsFederationMessage,
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

                authFailedEx.Throw();
            }

            return null;
        }

        private static AuthenticationTicket GetHandledResponseTicket()
        {
            return new AuthenticationTicket(null, new AuthenticationProperties(new Dictionary<string, string>() { { HandledResponse, "true" } }));
        }

        private AuthenticationProperties GetPropertiesFromWctx(string state)
        {
            AuthenticationProperties properties = null;
            if (!string.IsNullOrEmpty(state))
            {
                var pairs = ParseDelimited(state);
                List<string> values;
                if (pairs.TryGetValue(WsFederationAuthenticationDefaults.WctxKey, out values) && values.Count > 0)
                {
                    string value = values.First();
                    properties = Options.StateDataFormat.Unprotect(value);
                }
            }
            return properties;
        }

        private static IDictionary<string, List<string>> ParseDelimited(string text)
        {
            char[] delimiters = new[] { '&', ';' };
            var accumulator = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            int textLength = text.Length;
            int equalIndex = text.IndexOf('=');
            if (equalIndex == -1)
            {
                equalIndex = textLength;
            }
            int scanIndex = 0;
            while (scanIndex < textLength)
            {
                int delimiterIndex = text.IndexOfAny(delimiters, scanIndex);
                if (delimiterIndex == -1)
                {
                    delimiterIndex = textLength;
                }
                if (equalIndex < delimiterIndex)
                {
                    while (scanIndex != equalIndex && char.IsWhiteSpace(text[scanIndex]))
                    {
                        ++scanIndex;
                    }
                    string name = text.Substring(scanIndex, equalIndex - scanIndex);
                    string value = text.Substring(equalIndex + 1, delimiterIndex - equalIndex - 1);

                    name = Uri.UnescapeDataString(name.Replace('+', ' '));
                    value = Uri.UnescapeDataString(value.Replace('+', ' '));

                    List<string> existing;
                    if (!accumulator.TryGetValue(name, out existing))
                    {
                        accumulator.Add(name, new List<string>(1) { value });
                    }
                    else
                    {
                        existing.Add(value);
                    }

                    equalIndex = text.IndexOf('=', delimiterIndex);
                    if (equalIndex == -1)
                    {
                        equalIndex = textLength;
                    }
                }
                scanIndex = delimiterIndex + 1;
            }
            return accumulator;
        }
    }
}