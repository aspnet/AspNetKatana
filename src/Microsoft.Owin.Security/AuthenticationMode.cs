// <copyright file="AuthenticationMode.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// Controls the behavior of authentication middleware
    /// </summary>
    public enum AuthenticationMode
    {
        /// <summary>
        /// In Active mode the authentication middleware will alter the user identity as the request arrives, and
        /// will also alter a plain 401 as the response leaves.
        /// </summary>
        Active,

        /// <summary>
        /// In Passive mode the authentication middleware will only provide user identity when asked, and will only
        /// alter 401 responses where the authentication type named in the extra challenge data.
        /// </summary>
        Passive
    }
}
