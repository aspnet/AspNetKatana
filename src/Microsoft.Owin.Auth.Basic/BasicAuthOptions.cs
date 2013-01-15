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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Owin.Auth.Basic
{
    using AuthCallback = Func<IDictionary<string, object> /*env*/, string /*user*/, string /*psw*/, Task<bool>>;

    public class BasicAuthOptions
    {
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public BasicAuthOptions(AuthCallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            Authenticate = callback;
        }

        public string Realm { get; set; }
        public bool RequireEncryption { get; set; }
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public AuthCallback Authenticate { get; private set; }
    }
}
