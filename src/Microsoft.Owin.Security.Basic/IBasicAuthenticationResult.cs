// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Principal;

namespace Microsoft.Owin.Security.Basic
{
    internal interface IBasicAuthenticationResult
    {
        IPrincipal Principal { get; }

        IBasicAuthenticationError ErrorResult { get; }
    }
}
