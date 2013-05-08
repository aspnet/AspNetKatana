// <copyright file="DpapiDataProtectionProvider.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security.DataProtection
{
    /// <summary>
    /// Used to provide the data protection services that are derived from the Data Protection API. It is the best choice of
    /// data protection when you application is not hosted by ASP.NET and all processes are running as the same domain identity. 
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dpapi", Justification = "Acronym")]
    public class DpapiDataProtectionProvider : IDataProtectionProvider
    {
        /// <summary>
        /// Returns a new instance of IDataProtection for the provider.
        /// </summary>
        /// <param name="purposes">Additional entropy used to ensure protected data may only be unprotected for the correct purposes.</param>
        /// <returns>An instance of a data protection service</returns>
        public IDataProtector Create(params string[] purposes)
        {
            return new DpapiDataProtector(purposes);
        }
    }
}
