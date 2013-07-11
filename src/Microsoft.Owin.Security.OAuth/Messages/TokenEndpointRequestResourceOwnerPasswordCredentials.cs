// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.OAuth.Messages
{
    public class TokenEndpointRequestResourceOwnerPasswordCredentials
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Scope { get; set; }
    }
}
