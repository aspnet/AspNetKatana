// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security.OAuth.Messages
{
    /// <summary>
    /// Data object used by TokenEndpointRequest when the "grant_type" parameter is "refresh_token".
    /// </summary>
    public class TokenEndpointRequestRefreshToken
    {
        /// <summary>
        /// The value passed to the Token endpoint in the "refresh_token" parameter
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// The value passed to the Token endpoint in the "scope" parameter
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is just a data container object.")]
        public IList<string> Scope { get; set; }
    }
}
