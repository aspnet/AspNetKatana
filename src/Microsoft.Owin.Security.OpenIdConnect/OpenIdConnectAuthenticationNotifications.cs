// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security.Notifications;

namespace Microsoft.Owin.Security.OpenIdConnect
{
    public class OpenIdConnectAuthenticationNotifications
    {
        public Func<AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>, Task> AuthenticationFailed { get; set; }
        public Func<AccessCodeReceivedNotification, Task> AccessCodeReceived { get; set; }
        public Func<MessageReceivedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>, Task> MessageReceived { get; set; }
        public Func<RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>, Task> RedirectToIdentityProvider { get; set; }
        public Func<SecurityTokenReceivedNotification<OpenIdConnectAuthenticationOptions>, Task> SecurityTokenReceived { get; set; }
        public Func<SecurityTokenValidatedNotification<OpenIdConnectAuthenticationOptions>, Task> SecurityTokenValidated { get; set; }

        public Func<Task> SignedIn { get; set; }
        public Func<Task> SignedOut { get; set; }
    }
}