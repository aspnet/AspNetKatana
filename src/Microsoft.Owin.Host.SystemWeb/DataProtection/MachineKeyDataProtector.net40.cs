// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET40

using System.Text;
using System.Web.Security;

namespace Microsoft.Owin.Host.SystemWeb.DataProtection
{
    internal partial class MachineKeyDataProtector
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "purposes", Justification = "This will be used soon")]
        public MachineKeyDataProtector(params string[] purposes)
        {
        }

        public virtual byte[] Protect(byte[] userData)
        {
            return Encoding.UTF8.GetBytes(MachineKey.Encode(userData, MachineKeyProtection.All));
        }

        public virtual byte[] Unprotect(byte[] protectedData)
        {
            return MachineKey.Decode(Encoding.UTF8.GetString(protectedData), MachineKeyProtection.All);
        }
    }
}

#else

using FormattingWorkaround = System.Object;

#endif
