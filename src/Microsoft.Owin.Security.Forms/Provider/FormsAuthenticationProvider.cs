// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Forms
{
    /// <summary>
    /// This default implementation of the IFormsAuthenticationProvider may be used if the 
    /// application only needs to override a few of the interface methods. This may be used as a base class
    /// or may be instantiated directly.
    /// </summary>
    public class FormsAuthenticationProvider : IFormsAuthenticationProvider
    {
        /// <summary>
        /// Create a new instance of the default provider.
        /// </summary>
        public FormsAuthenticationProvider()
        {
            OnValidateIdentity = context => Task.FromResult<object>(null);
            OnResponseSignIn = context => { };
        }

        /// <summary>
        /// A delegate assigned to this property will be invoked when the related method is called
        /// </summary>
        public Func<FormsValidateIdentityContext, Task> OnValidateIdentity { get; set; }

        /// <summary>
        /// A delegate assigned to this property will be invoked when the related method is called
        /// </summary>
        public Action<FormsResponseSignInContext> OnResponseSignIn { get; set; }

        /// <summary>
        /// Implements the interface method by invoking the related delegate method
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual Task ValidateIdentity(FormsValidateIdentityContext context)
        {
            return OnValidateIdentity.Invoke(context);
        }

        /// <summary>
        /// Implements the interface method by invoking the related delegate method
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual void ResponseSignIn(FormsResponseSignInContext context)
        {
            OnResponseSignIn.Invoke(context);
        }
    }
}
