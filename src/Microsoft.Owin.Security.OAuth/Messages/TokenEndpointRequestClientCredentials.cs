// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security.OAuth.Messages
{
    /// <summary>
    /// Data object used by TokenEndpointRequest when the "grant_type" is "client_credentials".
    /// </summary>    
    public class TokenEndpointRequestClientCredentials
    {
        /// <summary>
        /// The value passed to the Token endpoint in the "scope" parameter
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This class is just for passing data through.")]
        public IList<string> Scope { get; set; }
    }
}
