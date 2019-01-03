// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        /// <param name="accessToken">Google OAuth 2.0 access token</param>
        /// <param name="refreshToken">Goolge OAuth 2.0 refresh token</param>
        /// <param name="expires">Seconds until expiration</param>
        public GoogleOAuth2AuthenticatedContext(IOwinContext context, JObject user, string accessToken, 
            string refreshToken, string expires)
            : base(context)
        {
            User = user;
            AccessToken = accessToken;
            RefreshToken = refreshToken;

            int expiresValue;
            if (Int32.TryParse(expires, NumberStyles.Integer, CultureInfo.InvariantCulture, out expiresValue))
            {
                ExpiresIn = TimeSpan.FromSeconds(expiresValue);
            }

            Id = TryGetValue(user, "id");
            Name = TryGetValue(user, "name");
            GivenName = TryGetValue(user, "given_name");
            FamilyName = TryGetValue(user, "family_name");
            Profile = TryGetValue(user, "link");
            Email = TryGetValue(user, "email");
        }

        /// <summary>
        /// Initializes a <see cref="GoogleOAuth2AuthenticatedContext"/>
        /// </summary>
        /// <param name="context">The OWIN environment</param>
        /// <param name="user">The JSON-serialized Google user info</param>
        /// <param name="tokenResponse">The JSON-serialized token response Google</param>
        public GoogleOAuth2AuthenticatedContext(IOwinContext context, JObject user, JObject tokenResponse)
            : base(context)
        {
            User = user;
            TokenResponse = tokenResponse;
            if (tokenResponse != null)
            {
                AccessToken = tokenResponse.Value<string>("access_token");
                RefreshToken = tokenResponse.Value<string>("refresh_token");

                int expiresValue;
                if (Int32.TryParse(tokenResponse.Value<string>("expires_in"), NumberStyles.Integer, CultureInfo.InvariantCulture, out expiresValue))
                {
                    ExpiresIn = TimeSpan.FromSeconds(expiresValue);
                }
            }

            Id = TryGetValue(user, "id");
            Name = TryGetValue(user, "name");
            GivenName = TryGetValue(user, "given_name");
            FamilyName = TryGetValue(user, "family_name");
            Profile = TryGetValue(user, "link");
            Email = TryGetValue(user, "email");
        }

        /// <summary>
        /// Gets the JSON-serialized user
        /// </summary>
        /// <remarks>
        /// Contains the Google user obtained from the UserInformationEndpoint
        /// </remarks>
        public JObject User { get; private set; }

        /// <summary>
        /// Gets the Google access token
        /// </summary>
        public string AccessToken { get; private set; }

        /// <summary>
        /// Gets the Google refresh token
        /// </summary>
        /// <remarks>
        /// This value is not null only when access_type authorize parameter is offline.
        /// </remarks>
        public string RefreshToken { get; private set; }

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
        /// Token response from Google
        /// </summary>
        public JObject TokenResponse { get; private set; }

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
