// <copyright file="MicrosoftAccountAuthenticatedContext.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Linq;
using System.Security.Claims;

using Microsoft.Owin.Security.Provider;

using Newtonsoft.Json.Linq;

namespace Microsoft.Owin.Security.MicrosoftAccount
{
    /// <summary>
    /// Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.
    /// </summary>
    public class MicrosoftAccountAuthenticatedContext : BaseContext
    {
        /// <summary>
        /// Initializes a <see cref="MicrosoftAccountAuthenticatedContext"/>
        /// </summary>
        /// <param name="context">The OWIN environment</param>
        /// <param name="user">The JSON-serialized user</param>
        /// <param name="accessToken">The access token provided by the Microsoft authentication service</param>
        public MicrosoftAccountAuthenticatedContext(IOwinContext context, JObject user, string accessToken)
            : base(context)
        {
            IDictionary<string, JToken> userAsDictionary = user;

            User = user;
            AccessToken = accessToken;

            Id = User["id"].ToString();
            Name = PropertyValueIfExists("name", userAsDictionary);
            FirstName = PropertyValueIfExists("first_name", userAsDictionary);
            LastName = PropertyValueIfExists("last_name", userAsDictionary);

            if (userAsDictionary.ContainsKey("emails"))
            {
                var emailsNode = user["emails"];
                foreach (var childAsProperty in emailsNode.OfType<JProperty>().Where(childAsProperty => childAsProperty.Name == "preferred"))
                {
                    this.Email = childAsProperty.Value.ToString();
                }
            }
        }

        /// <summary>
        /// Gets the JSON-serialized user
        /// </summary>
        public JObject User { get; private set; }

        /// <summary>
        /// Gets the access token provided by the Microsoft authenication service
        /// </summary>
        public string AccessToken { get; private set; }

        /// <summary>
        /// Gets the Microsoft Account user ID
        /// </summary>
        public string Id { get; private set; }
        
        /// <summary>
        /// Gets the user name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the user first name
        /// </summary>
        public string FirstName { get; private set; }

        /// <summary>
        /// Gets the user last name
        /// </summary>
        public string LastName { get; private set; }

        /// <summary>
        /// Gets the user email address
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Gets the <see cref="ClaimsIdentity"/> representing the user
        /// </summary>
        public ClaimsIdentity Identity { get; set; }

        /// <summary>
        /// Gets or sets a property bag for common authentication properties
        /// </summary>
        public AuthenticationProperties Properties { get; set; }

        private static string PropertyValueIfExists(string property, IDictionary<string, JToken> dictionary)
        {
            return dictionary.ContainsKey(property) ? dictionary[property].ToString() : null;
        }
    }
}
