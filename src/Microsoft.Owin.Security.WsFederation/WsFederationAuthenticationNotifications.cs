// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security.Notifications;

namespace Microsoft.Owin.Security.WsFederation
{
    /// <summary>
    /// Specifies events which the <see cref="WsFederationAuthenticationMiddleware"></see> invokes to enable developer control over the authentication process. />
    /// </summary>
    public class WsFederationAuthenticationNotifications
    {
        /// <summary>
        /// Creates a new set of notifications. Each notification has a default no-op behavior unless otherwise documented.
        /// </summary>
        public WsFederationAuthenticationNotifications()
        {
            AuthenticationFailed = notification => Task.FromResult(0);
            MessageReceived = notification => Task.FromResult(0);
            SecurityTokenReceived = notification => Task.FromResult(0);
            SecurityTokenValidated = notification => Task.FromResult(0);
            ApplyRedirectToIdentityProvider = notification =>
            {
                string redirectUri;
                if (notification.ProtocolMessage.IsSignInMessage)
                {
                    redirectUri = notification.ProtocolMessage.CreateSignInUrl();
                }
                else
                {
                    // IsSignOutMessage
                    redirectUri = notification.ProtocolMessage.CreateSignOutUrl();
                }
                if (Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
                {
                    // TODO: else log error?
                    notification.Response.Redirect(redirectUri);
                }
                return Task.FromResult(0);
            };
        }

        /// <summary>
        /// Invoked if exceptions are thrown during request processing. The exceptions will be re-thrown after this event unless suppressed.
        /// </summary>
        public Func<AuthenticationFailedNotification<WsFederationMessage, WsFederationAuthenticationOptions>, Task> AuthenticationFailed { get; set; }

        /// <summary>
        /// Invoked when a protocol message is first received.
        /// </summary>
        public Func<MessageReceivedNotification<WsFederationMessage, WsFederationAuthenticationOptions>, Task> MessageReceived { get; set; }

        /// <summary>
        /// Invoked to generate redirects to the identity provider for SignIn, SignOut, or Challenge. This event has a default implementation.
        /// </summary>
        public Func<RedirectToIdentityProviderNotification<WsFederationMessage, WsFederationAuthenticationOptions>, Task> ApplyRedirectToIdentityProvider { get; set; }

        /// <summary>
        /// Invoked with the security token that has been extracted from the protocol message.
        /// </summary>
        public Func<SecurityTokenReceivedNotification<WsFederationMessage, WsFederationAuthenticationOptions>, Task> SecurityTokenReceived { get; set; }

        /// <summary>
        /// Invoked after the security token has passed validation and a ClaimsIdentity has been generated.
        /// </summary>
        public Func<SecurityTokenValidatedNotification<WsFederationMessage, WsFederationAuthenticationOptions>, Task> SecurityTokenValidated { get; set; }
    }
}
