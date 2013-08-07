// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.OAuth.Messages
{
    /// <summary>
    /// Data object used by TokenEndpointRequest which contains parameter information when the "grant_type" is unrecognized.
    /// </summary>
    public class TokenEndpointRequestCustomExtension
    {
        /// <summary>
        /// The parameter information when the "grant_type" is unrecognized.
        /// </summary>
        public IReadableStringCollection Parameters { get; set; }
    }
}
