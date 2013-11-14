// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Security.Claims;
using Microsoft.Owin.Security.Provider;
using Newtonsoft.Json.Linq;

namespace Microsoft.Owin.Security.Google
{
    /// <summary>
    /// Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Auth",
        Justification = "OAuth2 is a valid word.")]
    public class GoogleOAuth2AuthenticatedContext : BaseContext
    {
        /// <summary>
        /// Initializes a <see cref="GoogleOAuth2AuthenticatedContext"/>
        /// </summary>
        /// <param name="context">The OWIN environment</param>
        /// <param name="user">The JSON-serialized Google user info</param>
        /// <param name="accessToken">Google+ Access token</param>
        /// <param name="expires">Seconds until expiration</param>
        public GoogleOAuth2AuthenticatedContext(IOwinContext context, JObject user, string accessToken, string expires)
            : base(context)
        {
            User = user;
            AccessToken = accessToken;

            int expiresValue;
            if (Int32.TryParse(expires, NumberStyles.Integer, CultureInfo.InvariantCulture, out expiresValue))
            {
                ExpiresIn = TimeSpan.FromSeconds(expiresValue);
            }

            Id = TryGetValue(user, "sub");
            Name = TryGetValue(user, "name");
            GivenName = TryGetValue(user, "given_name");
            FamilyName = TryGetValue(user, "family_name");
            Profile = TryGetValue(user, "profile");
            Email = TryGetValue(user, "email");
        }

        /// <summary>
        /// Gets the JSON-serialized user
        /// </summary>
        /// <remarks>
        /// Contains the Google user obtained from the endpoint https://www.googleapis.com/oauth2/v3/userinfo
        /// </remarks>
        public JObject User { get; private set; }

        /// <summary>
        /// Gets the Google access token
        /// </summary>
        public string AccessToken { get; private set; }

        /// <summary>
        /// Gets the Google access token expiration time
        /// </summary>
        public TimeSpan? ExpiresIn { get; set; }

        /// <summary>
        /// Gets the Google user ID
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the user's name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the user's given name
        /// </summary>
        public string GivenName { get; set; }

        /// <summary>
        /// Gets the user's family name
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets the user's profile link
        /// </summary>
        public string Profile { get; private set; }

        /// <summary>
        /// Gets the user's email
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

        private static string TryGetValue(JObject user, string propertyName)
        {
            JToken value;
            return user.TryGetValue(propertyName, out value) ? value.ToString() : null;
        }
    }
}
