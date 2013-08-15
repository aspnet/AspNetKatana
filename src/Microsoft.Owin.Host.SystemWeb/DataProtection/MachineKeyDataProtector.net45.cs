// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

using System.Text;
using System.Web.Security;

namespace Microsoft.Owin.Host.SystemWeb.DataProtection
{
    internal partial class MachineKeyDataProtector
    {
        private readonly string[] _purposes;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "purposes", Justification = "This will be used soon")]
        public MachineKeyDataProtector(params string[] purposes)
        {
            _purposes = purposes;
        }

        public virtual byte[] Protect(byte[] userData)
        {
            return MachineKey.Protect(userData, _purposes);
        }

        public virtual byte[] Unprotect(byte[] protectedData)
        {
            return MachineKey.Unprotect(protectedData, _purposes);
        }
    }
}

#else

using FormattingWorkaround = System.Object;

#endif
