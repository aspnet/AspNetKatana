// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.Basic
{
    /// <summary></summary>
    public class BasicAuthenticationOptions
    {
        /// <summary></summary>
        public IBasicAuthenticationProvider Provider { get; set; }

        /// <summary></summary>
        public string Realm { get; set; }
    }
}
