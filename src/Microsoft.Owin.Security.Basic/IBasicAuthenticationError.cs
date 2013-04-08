// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;

namespace Microsoft.Owin.Security.Basic
{
    internal interface IBasicAuthenticationError
    {
        HttpStatusCode StatusCode { get; }

        string Message { get; }
    }
}
