// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
