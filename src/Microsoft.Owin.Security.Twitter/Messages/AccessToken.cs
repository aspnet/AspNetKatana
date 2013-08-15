// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.Twitter.Messages
{
    /// <summary>
    /// Twitter access token
    /// </summary>
    public class AccessToken : RequestToken
    {
        /// <summary>
        /// Gets or sets the Twitter User ID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the Twitter screen name
        /// </summary>
        public string ScreenName { get; set; }
    }
}
