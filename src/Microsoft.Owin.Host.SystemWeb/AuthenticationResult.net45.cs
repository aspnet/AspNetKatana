// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Microsoft.Owin.Host.SystemWeb
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthenticationResult
    {
        private readonly IIdentity _identity;
        private readonly IDictionary<string, string> _extra;
        private readonly IDictionary<string, object> _properties;

        /// <summary></summary>
        /// <param name="identity"></param>
        /// <param name="extra"></param>
        /// <param name="properties"></param>
        public AuthenticationResult(IIdentity identity, IDictionary<string, string> extra, IDictionary<string, object> properties)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            if (extra == null)
            {
                throw new ArgumentNullException("extra");
            }

            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            _identity = identity;
            _extra = extra;
            _properties = properties;
        }

        /// <summary></summary>
        public IIdentity Identity
        {
            get { return _identity; }
        }

        /// <summary></summary>
        public IDictionary<string, string> Extra
        {
            get { return _extra; }
        }

        /// <summary></summary>
        public IDictionary<string, object> Properties
        {
            get { return _properties; }
        }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
