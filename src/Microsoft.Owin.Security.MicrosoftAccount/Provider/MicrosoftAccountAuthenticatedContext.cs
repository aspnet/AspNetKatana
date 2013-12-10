// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
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
        /// <param name="refreshToken">The refresh token provided by Microsoft authentication service</param>
        /// <param name="expires">Seconds until expiration</param>
        public MicrosoftAccountAuthenticatedContext(IOwinContext context, JObject user, string accessToken, 
            string refreshToken, string expires)
            : base(context)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            IDictionary<string, JToken> userAsDictionary = user;

            User = user;
            AccessToken = accessToken;
            RefreshToken = refreshToken;

            int expiresValue;
            if (Int32.TryParse(expires, NumberStyles.Integer, CultureInfo.InvariantCulture, out expiresValue))
            {
                ExpiresIn = TimeSpan.FromSeconds(expiresValue);
            }

            JToken userId = User["id"];
            if (userId == null)
            {
                throw new ArgumentException(Resources.Exception_MissingId, "user");
            }

            Id = userId.ToString();
            Name = PropertyValueIfExists("name", userAsDictionary);
            FirstName = PropertyValueIfExists("first_name", userAsDictionary);
            LastName = PropertyValueIfExists("last_name", userAsDictionary);

            if (userAsDictionary.ContainsKey("emails"))
            {
                JToken emailsNode = user["emails"];
                foreach (var childAsProperty in emailsNode.OfType<JProperty>().Where(childAsProperty => childAsProperty.Name == "preferred"))
                {
                    Email = childAsProperty.Value.ToString();
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
        /// Gets the refresh token provided by Microsoft authentication service
        /// </summary>
        /// <remarks>
        /// Refresh token is only available when wl.offline_access is request.
        /// Otherwise, it is null.
        /// </remarks>
        public string RefreshToken { get; private set; }

        /// <summary>
        /// Gets the Microsoft access token expiration time
        /// </summary>
        public TimeSpan? ExpiresIn { get; set; }

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
