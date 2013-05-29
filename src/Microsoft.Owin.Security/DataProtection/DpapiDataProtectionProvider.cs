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

using System;

namespace Microsoft.Owin.Security.DataProtection
{
    /// <summary>
    /// Used to provide the data protection services that are derived from the Data Protection API. It is the best choice of
    /// data protection when you application is not hosted by ASP.NET and all processes are running as the same domain identity. 
    /// </summary>
    public class DpapiDataProtectionProvider : IDataProtectionProvider
    {
        private readonly string _appName;

        /// <summary>
        /// Initializes a new DpapiDataProtectionProvider with a random application
        /// name. This is only useful to protect data for the duration of the
        /// current application execution.
        /// </summary>
        public DpapiDataProtectionProvider() : this(Guid.NewGuid().ToString())
        {
        }

        /// <summary>
        /// Initializes a new DpapiDataProtectionProvider which uses the given
        /// appName as part of the protection algorithm
        /// </summary>
        /// <param name="appName">A user provided value needed to round-trip secured
        /// data. The default value comes from the IAppBuilder.Properties["owin.AppName"] 
        /// when self-hosted.</param>
        public DpapiDataProtectionProvider(string appName)
        {
            if (appName == null)
            {
                throw new ArgumentNullException("appName");
            }
            _appName = appName;
        }

        /// <summary>
        /// Returns a new instance of IDataProtection for the provider.
        /// </summary>
        /// <param name="purposes">Additional entropy used to ensure protected data may only be unprotected for the correct purposes.</param>
        /// <returns>An instance of a data protection service</returns>
        public IDataProtector Create(params string[] purposes)
        {
            if (purposes == null)
            {
                throw new ArgumentNullException("purposes");
            }
            return new DpapiDataProtector(_appName, purposes);
        }
    }
}
