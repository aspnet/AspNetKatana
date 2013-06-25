// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#if !NET40
using Microsoft.Owin.Security;
#endif

namespace Microsoft.Owin
{
    public interface IOwinContext
    {
        IOwinRequest Request { get; }
        IOwinResponse Response { get; }

        IDictionary<string, object> Environment { get; }

#if !NET40
        IAuthenticationManager Authentication { get; }
#endif

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Re-evaluate later.")]
        T Get<T>(string key);
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set", Justification = "Re-evaluate later.")]
        IOwinContext Set<T>(string key, T value);
    }
}
