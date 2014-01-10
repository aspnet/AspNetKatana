// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthBearerAuthenticationProvider : IOAuthBearerAuthenticationProvider
    {
        public OAuthBearerAuthenticationProvider()
        {
            OnRequestToken = context => Task.FromResult<object>(null);
            OnValidateIdentity = context => Task.FromResult<object>(null);
            OnApplyChallenge = context =>
            {
                context.OwinContext.Response.Headers.AppendValues("WWW-Authenticate", context.Challenge);
                return Task.FromResult(0);
            };
        }

        public Func<OAuthRequestTokenContext, Task> OnRequestToken { get; set; }
        public Func<OAuthValidateIdentityContext, Task> OnValidateIdentity { get; set; }
        public Func<OAuthChallengeContext, Task> OnApplyChallenge { get; set; }

        public virtual Task RequestToken(OAuthRequestTokenContext context)
        {
            return OnRequestToken(context);
        }

        public virtual Task ValidateIdentity(OAuthValidateIdentityContext context)
        {
            return OnValidateIdentity.Invoke(context);
        }

        public Task ApplyChallenge(OAuthChallengeContext context)
        {
            return OnApplyChallenge(context);
        }
    }
}
