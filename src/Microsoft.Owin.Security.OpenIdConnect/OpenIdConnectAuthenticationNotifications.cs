// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security.Notifications;

namespace Microsoft.Owin.Security.OpenIdConnect
{
    public class OpenIdConnectAuthenticationNotifications
    {
        public OpenIdConnectAuthenticationNotifications()
        {
            AuthenticationFailed = notification => Task.FromResult(0);
            AuthorizationCodeReceived = notification => Task.FromResult(0);
            MessageReceived = notification => Task.FromResult(0);
            SecurityTokenReceived = notification => Task.FromResult(0);
            SecurityTokenValidated = notification => Task.FromResult(0);
            ApplyRedirectToIdentityProvider = notification =>
            {
                string redirectUri;
                if (notification.ProtocolMessage.RequestType == OpenIdConnectRequestType.AuthenticationRequest)
                {
                    redirectUri = notification.ProtocolMessage.BuildRedirectUrl();
                }
                else
                {
                    // LogoutRequest
                    redirectUri = notification.ProtocolMessage.CreateLogoutUrl();
                }
                if (Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
                {
                    // TODO: else log error?
                    notification.Response.Redirect(redirectUri);
                }
                return Task.FromResult(0);
            };
        }

        public Func<AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>, Task> AuthenticationFailed { get; set; }
        public Func<AuthorizationCodeReceivedNotification, Task> AuthorizationCodeReceived { get; set; }
        public Func<MessageReceivedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>, Task> MessageReceived { get; set; }
        public Func<RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>, Task> ApplyRedirectToIdentityProvider { get; set; }
        public Func<SecurityTokenReceivedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>, Task> SecurityTokenReceived { get; set; }
        public Func<SecurityTokenValidatedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>, Task> SecurityTokenValidated { get; set; }

        public Func<Task> SignedIn { get; set; }
        public Func<Task> SignedOut { get; set; }
    }
}