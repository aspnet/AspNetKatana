// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.AspNet.Security;

namespace Microsoft.Owin.Security
{
    /// <summary></summary>
    public class BasicAuthenticationOptions
    {
        private IBasicAuthenticationProtocol _protocol;

        /// <summary></summary>
        public IBasicAuthenticationProtocol Protocol
        {
            get
            {
                if (_protocol == null && Provider != null)
                {
                    _protocol = new BasicAuthenticationProtocol(Provider, Realm);
                }

                return _protocol;
            }
            set
            {
                _protocol = value;
            }
        }

        /// <summary></summary>
        public IBasicAuthenticationProvider Provider { get; set; }

        /// <summary></summary>
        public string Realm { get; set; }
    }
}
