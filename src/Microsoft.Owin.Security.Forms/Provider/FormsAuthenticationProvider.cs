// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Forms
{
    public class FormsAuthenticationProvider : IFormsAuthenticationProvider
    {
        public FormsAuthenticationProvider()
        {
            OnValidateIdentity = context => Task.FromResult<object>(null);
            OnResponseSignIn = context => { };
        }

        public Func<FormsValidateIdentityContext, Task> OnValidateIdentity { get; set; }

        public Action<FormsResponseSignInContext> OnResponseSignIn { get; set; }

        public virtual Task ValidateIdentity(FormsValidateIdentityContext context)
        {
            return OnValidateIdentity.Invoke(context);
        }

        public virtual void ResponseSignIn(FormsResponseSignInContext context)
        {
            OnResponseSignIn.Invoke(context);
        }
    }
}
