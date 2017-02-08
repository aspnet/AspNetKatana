// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Owin;

namespace Microsoft.Owin.Security.DataProtection
{
    using DataProtectionProviderDelegate = Func<string[], Tuple<Func<byte[], byte[]>, Func<byte[], byte[]>>>;
    using DataProtectionTuple = Tuple<Func<byte[], byte[]>, Func<byte[], byte[]>>;

    public static class AppBuilderExtensions
    {
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
            if (app.Properties.TryGetValue("security.DataProtectionProvider", out value))
            {
                var del = value as DataProtectionProviderDelegate;
                if (del != null)
                {
                    return new CallDataProtectionProvider(del);
                }
            }
            return null;
        }

        public static IDataProtector CreateDataProtector(this IAppBuilder app, params string[] purposes)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }

            IDataProtectionProvider dataProtectionProvider = GetDataProtectionProvider(app);
            if (dataProtectionProvider == null)
            {
                dataProtectionProvider = FallbackDataProtectionProvider(app);
            }
            return dataProtectionProvider.Create(purposes);
        }

        private static IDataProtectionProvider FallbackDataProtectionProvider(IAppBuilder app)
        {
            return new DpapiDataProtectionProvider(GetAppName(app));
        }

        private static string GetAppName(IAppBuilder app)
        {
            object value;
            if (app.Properties.TryGetValue("host.AppName", out value))
            {
                var appName = value as string;
                if (!string.IsNullOrEmpty(appName))
                {
                    return appName;
                }
            }
            throw new NotSupportedException(Resources.Exception_DefaultDpapiRequiresAppNameKey);
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
