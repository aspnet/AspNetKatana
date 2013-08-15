// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Security.DataHandler.Serializer;

namespace Microsoft.Owin.Security.Twitter.Messages
{
    /// <summary>
    /// Provides access to a request token serializer
    /// </summary>
    public static class Serializers
    {
        static Serializers()
        {
            RequestToken = new RequestTokenSerializer();
        }

        /// <summary>
        /// Gets or sets a statically-avaliable serializer object. The value for this property will be <see cref="RequestTokenSerializer"/> by default.
        /// </summary>
        public static IDataSerializer<RequestToken> RequestToken { get; set; }
    }
}
