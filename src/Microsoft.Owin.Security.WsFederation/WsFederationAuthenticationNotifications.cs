// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;

namespace Microsoft.Owin.Security.WsFederation
{
    public class WsFederationAuthenticationNotifications
    {
        public Func<AuthenticationFailedNotification<WsFederationMessage>, Task> AuthenticationFailed { get; set; }
        public Func<MessageReceivedNotification<WsFederationMessage>, Task> MessageReceived { get; set; }
        public Func<RedirectToIdentityProviderNotification<WsFederationMessage>, Task> RedirectToIdentityProvider { get; set; }
        public Func<SecurityTokenReceivedNotification, Task> SecurityTokenReceived { get; set; }
        public Func<SecurityTokenValidatedNotification, Task> SecurityTokenValidated { get; set; }
    }
}
