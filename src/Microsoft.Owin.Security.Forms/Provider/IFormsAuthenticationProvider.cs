// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Forms
{
    public interface IFormsAuthenticationProvider
    {
        /// <summary>
        /// Called each time a request identity has been validated by the middleware. By implementing this method the
        /// application may alter or reject the identity which has arrived with the request.
        /// </summary>
        /// <param name="context">Contains information related to the operation being performed</param>
        /// <returns>Async completion</returns>
        Task ValidateIdentity(FormsValidateIdentityContext context);

        /// <summary>
        /// Called when an endpoint has provided sign in information before it is converted into a cookie. By
        /// implementing this method the claims and extra information that go into the ticket may be altered.
        /// </summary>
        /// <param name="context"></param>
        void ResponseSignIn(FormsResponseSignInContext context);
    }
}
