// <copyright file="Serializers.cs" company="Microsoft Open Technologies, Inc.">
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
        public static IDataSerializer<RequestToken> RequestToken
        {
            get;
            set;
        }
    }
}
