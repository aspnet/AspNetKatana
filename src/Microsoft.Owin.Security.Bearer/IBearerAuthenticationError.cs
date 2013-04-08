// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Microsoft.Owin.Security.Bearer
{
    /// <summary></summary>
    public interface IBearerAuthenticationError
    {
        /// <summary></summary>
        HttpStatusCode StatusCode { get; }

        /// <summary></summary>
        string Code { get; }

        /// <summary></summary>
        string Description { get; }

        /// <summary></summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings",
            Justification = "We want to support URIs as strings")]
        string Uri { get; }
    }
}
