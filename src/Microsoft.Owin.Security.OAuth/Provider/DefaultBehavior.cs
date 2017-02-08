// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.OAuth
{
    internal static class DefaultBehavior
    {
        internal static readonly Func<OAuthValidateAuthorizeRequestContext, Task> ValidateAuthorizeRequest = context =>
        {
            context.Validated();
            return Task.FromResult<object>(null);
        };

        internal static readonly Func<OAuthValidateTokenRequestContext, Task> ValidateTokenRequest = context =>
        {
            context.Validated();
            return Task.FromResult<object>(null);
        };

        internal static readonly Func<OAuthGrantAuthorizationCodeContext, Task> GrantAuthorizationCode = context =>
        {
            if (context.Ticket != null && context.Ticket.Identity != null && context.Ticket.Identity.IsAuthenticated)
            {
                context.Validated();
            }
            return Task.FromResult<object>(null);
        };

        internal static readonly Func<OAuthGrantRefreshTokenContext, Task> GrantRefreshToken = context =>
        {
            if (context.Ticket != null && context.Ticket.Identity != null && context.Ticket.Identity.IsAuthenticated)
            {
                context.Validated();
            }
            return Task.FromResult<object>(null);
        };
    }
}
