// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security.Notifications;

namespace Microsoft.Owin.Security.WsFederation
{
    public class WsFederationAuthenticationNotifications
    {
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

        public Func<AuthenticationFailedNotification<WsFederationMessage, WsFederationAuthenticationOptions>, Task> AuthenticationFailed { get; set; }
        public Func<MessageReceivedNotification<WsFederationMessage, WsFederationAuthenticationOptions>, Task> MessageReceived { get; set; }
        public Func<RedirectToIdentityProviderNotification<WsFederationMessage, WsFederationAuthenticationOptions>, Task> ApplyRedirectToIdentityProvider { get; set; }
        public Func<SecurityTokenReceivedNotification<WsFederationMessage, WsFederationAuthenticationOptions>, Task> SecurityTokenReceived { get; set; }
        public Func<SecurityTokenValidatedNotification<WsFederationMessage, WsFederationAuthenticationOptions>, Task> SecurityTokenValidated { get; set; }
    }
}
