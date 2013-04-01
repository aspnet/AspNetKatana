// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;

namespace Microsoft.AspNet.Security
{
    /// <summary></summary>
    public interface IBasicAuthenticationError
    {
        /// <summary></summary>
        HttpStatusCode StatusCode { get; }

        /// <summary></summary>
        string Message { get; }
    }
}
