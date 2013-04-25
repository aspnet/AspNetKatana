// <copyright file="SelectDefaultDataProtectionProvider.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Threading;
using Microsoft.Owin.Security.Infrastructure;

namespace Microsoft.Owin.Security.DataProtection
{
    /// <summary>
    /// A data protection provider that will call through to the MachineKeyDataProtectionProvider with hosted in ASP.NET,
    /// and calls through to the DpapiDataProtectionProvider in all other cases.
    /// </summary>
    public class SelectDefaultDataProtectionProvider : IDataProtectionProvider
    {
        private IDataProtectionProvider _provider;
        private bool _providerInitialized;
        private object _providerSyncLock;

        /// <summary>
        /// Returns a new instance of IDataProtection for the provider.
        /// </summary>
        /// <param name="purposes">Additional entropy used to ensure protected data may only be unprotected for the correct purposes.</param>
        /// <returns>An instance of a data protection service</returns>
        public IDataProtecter Create(params string[] purposes)
        {
            IDataProtectionProvider provider = LazyInitializer.EnsureInitialized(
                ref _provider,
                ref _providerInitialized,
                ref _providerSyncLock,
                SelectProvider);
            return provider.Create(purposes);
        }

        private IDataProtectionProvider SelectProvider()
        {
            if (HostingEnvironmentApi.IsHosted)
            {
                return new MachineKeyDataProtectionProvider();
            }
            else
            {
                return new DpapiDataProtectionProvider();
            }
        }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
