// <copyright file="CookieOptions.cs" company="Microsoft Open Technologies, Inc.">
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

using System;

namespace Microsoft.Owin
{
    /// <summary>
    /// Options used to create a new cookie.
    /// </summary>
    public class CookieOptions
    {
        /// <summary>
        /// Creates a default cookie with a path of '/'.
        /// </summary>
        public CookieOptions()
        {
            Path = "/";
        }

        /// <summary>
        /// The cookie domain.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// The cookie path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The cookie expiration date.
        /// </summary>
        public DateTime? Expires { get; set; }

        /// <summary>
        /// The cookie security requirement.
        /// </summary>
        public bool Secure { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool HttpOnly { get; set; }
    }
}
