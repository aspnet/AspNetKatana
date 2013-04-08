// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens;

namespace Microsoft.Owin.Security.Bearer
{
    /// <summary></summary>
    public class BearerAuthenticationOptions
    {
        private readonly SecurityTokenHandlerCollection _handlers = new SecurityTokenHandlerCollection();

        private IBearerAuthenticationProvider _provider;

        /// <summary></summary>
        public SecurityTokenHandlerCollection Handlers
        {
            get
            {
                return _handlers;
            }
        }

        /// <summary></summary>
        public IBearerAuthenticationProvider Provider
        {
            get
            {
                if (_provider == null && _handlers.Count > 0)
                {
                    _provider = new IdentityModelBearerAuthenticationProvider(Handlers);
                }

                return _provider;
            }
            set
            {
                _provider = value;
            }
        }

        /// <summary></summary>
        public string Realm { get; set; }
    }
}
