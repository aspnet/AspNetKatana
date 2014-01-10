// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.OAuth
{
    /// <summary>
    /// OAuth bearer token middleware provider
    /// </summary>
    public class OAuthBearerAuthenticationProvider : IOAuthBearerAuthenticationProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthBearerAuthenticationProvider"/> class
        /// </summary>
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

        /// <summary>
        /// Handles processing OAuth bearer token.
        /// </summary>
        public Func<OAuthRequestTokenContext, Task> OnRequestToken { get; set; }

        /// <summary>
        /// Handles validating the identity produced from an OAuth bearer token.
        /// </summary>
        public Func<OAuthValidateIdentityContext, Task> OnValidateIdentity { get; set; }
        public Func<OAuthChallengeContext, Task> OnApplyChallenge { get; set; }

        /// <summary>
        /// Handles processing OAuth bearer token.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual Task RequestToken(OAuthRequestTokenContext context)
        {
            return OnRequestToken(context);
        }

        /// <summary>
        /// Handles validating the identity produced from an OAuth bearer token.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
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
