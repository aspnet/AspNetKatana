// <copyright file="AuthenticationResponseChallenge.cs" company="Microsoft Open Technologies, Inc.">
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

#if NET45

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security
{
    public class AuthenticationResponseChallenge
    {
        public AuthenticationResponseChallenge(string[] authenticationTypes, AuthenticationExtra extra)
        {
            AuthenticationTypes = authenticationTypes;
            Extra = extra;
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "By design")]
        public string[] AuthenticationTypes { get; private set; }
        public AuthenticationExtra Extra { get; private set; }
    }
}

#endif
