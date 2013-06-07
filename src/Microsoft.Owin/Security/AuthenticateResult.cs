// <copyright file="AuthenticateResult.cs" company="Microsoft Open Technologies, Inc.">
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

#if NET45
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

namespace Microsoft.Owin.Security
{
    public class AuthenticateResult
    {
        public AuthenticateResult(IIdentity identity, IDictionary<string, string> extra, IDictionary<string, object> description)
        {
            if (identity != null)
            {
                Identity = identity as ClaimsIdentity ?? new ClaimsIdentity(identity);
            }
            Extra = new AuthenticationExtra(extra);
            Description = new AuthenticationDescription(description);
        }

        public ClaimsIdentity Identity { get; private set; }
        public AuthenticationExtra Extra { get; private set; }
        public AuthenticationDescription Description { get; private set; }
    }
}

#endif
