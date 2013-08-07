// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security.OAuth.Messages
{
    /// <summary>
    /// Data object used by TokenEndpointRequest when the "grant_type" is "password".
    /// </summary>    
    public class TokenEndpointRequestResourceOwnerPasswordCredentials
    {
        /// <summary>
        /// The value passed to the Token endpoint in the "username" parameter
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// The value passed to the Token endpoint in the "password" parameter
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The value passed to the Token endpoint in the "scope" parameter
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is just a data class.")]
        public IList<string> Scope { get; set; }
    }
}
