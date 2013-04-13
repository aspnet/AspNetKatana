// <copyright file="AuthenticationResult.net45.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

#if !NET40

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Microsoft.Owin.Host.SystemWeb
{
    /// <summary></summary>
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
