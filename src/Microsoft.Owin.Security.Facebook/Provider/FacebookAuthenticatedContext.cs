// <copyright file="FacebookAuthenticatedContext.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.Security.Provider;
using Newtonsoft.Json.Linq;

namespace Microsoft.Owin.Security.Facebook
{
    public class FacebookAuthenticatedContext : BaseContext
    {
        public FacebookAuthenticatedContext(IDictionary<string, object> environment, JObject user, string accessToken) : base(environment)
        {
            User = user;
            AccessToken = accessToken;

            Id = User["id"].ToString();
            Name = User["name"].ToString();
            Link = User["link"].ToString();
            Username = User["username"].ToString();
            Email = User["email"].ToString();
        }

        public JObject User { get; private set; }
        public string AccessToken { get; private set; }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Link { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }

        public ClaimsIdentity Identity { get; set; }
        public IDictionary<string, string> Extra { get; set; }
    }
}
