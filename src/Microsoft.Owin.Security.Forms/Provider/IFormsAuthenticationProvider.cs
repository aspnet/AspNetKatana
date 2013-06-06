// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Forms
{
    public interface IFormsAuthenticationProvider
    {
        Task ValidateIdentity(FormsValidateIdentityContext context);
        void ResponseSignIn(FormsResponseSignInContext context);
    }
}
