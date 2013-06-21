// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin
{
    public interface IReadableStringCollection : IEnumerable<KeyValuePair<string, string[]>>
    {
        string this[string key] { get; } // Joined
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Re-evaluate later.")]
        string Get(string key); // Joined
        IList<string> GetValues(string key); // Raw
    }
}
