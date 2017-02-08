// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Host.SystemWeb.DataProtection
{
    using DataProtectionProviderDelegate = Func<string[], Tuple<Func<byte[], byte[]>, Func<byte[], byte[]>>>;
    using DataProtectionTuple = Tuple<Func<byte[], byte[]>, Func<byte[], byte[]>>;

    /// <summary>
    /// Used to provide the data protection services that are derived from the MachineKey API. It is the best choice of
    /// data protection when you application is hosted by ASP.NET and all servers in the farm are running with the same Machine Key values.
    /// </summary>
    internal class MachineKeyDataProtectionProvider
    {
        /// <summary>
        /// Returns a new instance of IDataProtection for the provider.
        /// </summary>
        /// <param name="purposes">Additional entropy used to ensure protected data may only be unprotected for the correct purposes.</param>
        /// <returns>An instance of a data protection service</returns>
        public virtual MachineKeyDataProtector Create(params string[] purposes)
        {
            return new MachineKeyDataProtector(purposes);
        }

        public virtual DataProtectionProviderDelegate ToOwinFunction()
        {
            return purposes =>
            {
                MachineKeyDataProtector dataProtecter = Create(purposes);
                return new DataProtectionTuple(dataProtecter.Protect, dataProtecter.Unprotect);
            };
        }
    }
}
