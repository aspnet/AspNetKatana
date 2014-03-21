// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security.Notifications;

namespace Microsoft.Owin.Security.WsFederation
{
    public class WsFederationAuthenticationNotifications
    {
        public Func<AuthenticationFailedNotification<WsFederationMessage, WsFederationAuthenticationOptions>, Task> AuthenticationFailed { get; set; }
        public Func<MessageReceivedNotification<WsFederationMessage, WsFederationAuthenticationOptions>, Task> MessageReceived { get; set; }
        public Func<RedirectToIdentityProviderNotification<WsFederationMessage, WsFederationAuthenticationOptions>, Task> RedirectToIdentityProvider { get; set; }
        public Func<SecurityTokenReceivedNotification<WsFederationAuthenticationOptions>, Task> SecurityTokenReceived { get; set; }
        public Func<SecurityTokenValidatedNotification<WsFederationAuthenticationOptions>, Task> SecurityTokenValidated { get; set; }
    }
}
