// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.OAuth.Messages
{
    public class TokenEndpointRequestRefreshToken
    {
        public string RefreshToken { get; set; }
        public string Scope { get; set; }
    }
}
