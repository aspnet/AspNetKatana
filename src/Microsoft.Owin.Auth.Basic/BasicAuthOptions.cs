// <copyright file="BasicAuthOptions.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.Owin.Auth.Basic
{
    using AuthCallback = Func<IDictionary<string, object> /*env*/, string /*user*/, string /*psw*/, Task<bool>>;

    /// <summary>
    /// Authentication options for the BasicAuthMiddleware
    /// </summary>
    public class BasicAuthOptions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback">The user validation callback</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public BasicAuthOptions(AuthCallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            Authenticate = callback;
        }

        /// <summary>
        /// The Realm to append to the WWW-Authenticate challenge, if any
        /// </summary>
        public string Realm { get; set; }

        /// <summary>
        /// If enabled, this prevents request from performing Basic authentication over HTTP.  Only HTTPS will be allowed.
        /// </summary>
        public bool RequireEncryption { get; set; }

        /// <summary>
        /// The user validation callback
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public AuthCallback Authenticate { get; private set; }
    }
}
