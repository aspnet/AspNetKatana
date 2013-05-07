// <copyright file="AppBuilderExtensions.cs" company="Microsoft Open Technologies, Inc.">
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
using Owin;

namespace Microsoft.Owin.Security.DataProtection
{
    using DataProtectionProviderDelegate = Func<string[], Tuple<Func<byte[], byte[]>, Func<byte[], byte[]>>>;
    using DataProtectionTuple = Tuple<Func<byte[], byte[]>, Func<byte[], byte[]>>;

    public static class AppBuilderExtensions
    {
        private static readonly DpapiDataProtectionProvider FallbackDataProtectionProvider = new DpapiDataProtectionProvider();

        public static void SetDataProtectionProvider(this IAppBuilder app, IDataProtectionProvider dataProtectionProvider)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (dataProtectionProvider == null)
            {
                app.Properties.Remove("security.DataProtectionProvider");
            }
            else
            {
                app.Properties["security.DataProtectionProvider"] = new DataProtectionProviderDelegate(purposes =>
                {
                    IDataProtector dataProtection = dataProtectionProvider.Create(purposes);
                    return new DataProtectionTuple(dataProtection.Protect, dataProtection.Unprotect);
                });
            }
        }

        public static IDataProtectionProvider GetDataProtectionProvider(this IAppBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            object value;
            if (app.Properties.TryGetValue("security.DataProtectionProvider", out value) && value is DataProtectionProviderDelegate)
            {
                return new CallDataProtectionProvider(value as DataProtectionProviderDelegate);
            }
            return null;
        }

        public static IDataProtector CreateDataProtecter(this IAppBuilder app, params string[] purposes)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }

            IDataProtectionProvider dataProtectionProvider = GetDataProtectionProvider(app);
            if (dataProtectionProvider == null)
            {
                dataProtectionProvider = FallbackDataProtectionProvider;
            }
            return dataProtectionProvider.Create(purposes);
        }

        private class CallDataProtectionProvider : IDataProtectionProvider
        {
            private readonly DataProtectionProviderDelegate _create;

            public CallDataProtectionProvider(DataProtectionProviderDelegate create)
            {
                _create = create;
            }

            public IDataProtector Create(params string[] purposes)
            {
                DataProtectionTuple protection = _create.Invoke(purposes);
                return new CallDataProtection(protection.Item1, protection.Item2);
            }

            private class CallDataProtection : IDataProtector
            {
                private readonly Func<byte[], byte[]> _protect;
                private readonly Func<byte[], byte[]> _unprotect;

                public CallDataProtection(Func<byte[], byte[]> protect, Func<byte[], byte[]> unprotect)
                {
                    _protect = protect;
                    _unprotect = unprotect;
                }

                public byte[] Protect(byte[] userData)
                {
                    return _protect.Invoke(userData);
                }

                public byte[] Unprotect(byte[] protectedData)
                {
                    return _unprotect.Invoke(protectedData);
                }
            }
        }
    }
}
