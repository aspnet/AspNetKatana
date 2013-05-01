// <copyright file="FormsValidateLoginContext.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Microsoft.Owin.Security.Forms
{
    public class FormsValidateLoginContext
    {
        public FormsValidateLoginContext(IDictionary<string, object> environment, string authenticationType, string name, string password)
        {
            Environment = environment;
            AuthenticationType = authenticationType;
        }

        public IDictionary<string, object> Environment { get; set; }
        public string AuthenticationType { get; private set; }

        public IIdentity Identity { get; private set; }

        public void Signin(IIdentity identity)
        {
            Identity = identity;
        }

        public void Signin(string name, params Claim[] claims)
        {
            Signin(name, (IEnumerable<Claim>)claims);
        }

        public void Signin(string name, IEnumerable<Claim> claims)
        {
            Identity = new ClaimsIdentity(new GenericIdentity(name, AuthenticationType), claims);
        }
    }
}
