// <copyright file="FormsResponseSigninContext.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Owin.Security.Infrastructure;

namespace Microsoft.Owin.Security.Forms
{
    public class FormsResponseSignInContext
    {
        public FormsResponseSignInContext(IDictionary<string, object> environment, string authenticationType, ClaimsIdentity identity, AuthenticationExtra extra)
        {
            Environment = environment;
            AuthenticationType = authenticationType;
            Identity = identity;
            Extra = extra;
        }

        public IDictionary<string, object> Environment { get; set; }
        public string AuthenticationType { get; private set; }

        public ClaimsIdentity Identity { get; set; }
        public AuthenticationExtra Extra { get; set; }
    }
}
