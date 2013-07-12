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
        }

        public Func<OAuthRequestTokenContext, Task> OnRequestToken { get; set; }
        public Func<OAuthValidateIdentityContext, Task> OnValidateIdentity { get; set; }

        public virtual Task RequestToken(OAuthRequestTokenContext context)
        {
            return OnRequestToken(context);
        }

        public virtual Task ValidateIdentity(OAuthValidateIdentityContext context)
        {
            return OnValidateIdentity.Invoke(context);
        }
    }
}
