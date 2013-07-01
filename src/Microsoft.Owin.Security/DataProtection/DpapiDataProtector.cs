// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Cryptography;

namespace Microsoft.Owin.Security.DataProtection
{
    internal class DpapiDataProtector : IDataProtector
    {
        private readonly System.Security.Cryptography.DpapiDataProtector _protector;

        public DpapiDataProtector(string appName, string[] purposes)
        {
            _protector = new System.Security.Cryptography.DpapiDataProtector(appName, "Microsoft.Owin.Security.IDataProtector", purposes)
            {
                Scope = DataProtectionScope.CurrentUser
            };
        }

        public byte[] Protect(byte[] userData)
        {
            return _protector.Protect(userData);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return _protector.Unprotect(protectedData);
        }
    }
}
