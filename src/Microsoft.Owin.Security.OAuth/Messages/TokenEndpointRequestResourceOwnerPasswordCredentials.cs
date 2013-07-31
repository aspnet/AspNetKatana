// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security.OAuth.Messages
{
    public class TokenEndpointRequestResourceOwnerPasswordCredentials
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is just a data class.")]
        public IList<string> Scope { get; set; }
    }
}
