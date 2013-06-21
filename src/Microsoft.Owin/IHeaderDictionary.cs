// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin
{
    public interface IHeaderDictionary : IReadableStringCollection, IDictionary<string, string[]>
    {
        new string this[string key] { get; set; }
        IList<string> GetCommaSeparatedValues(string key); // Split
        void Append(string key, string value); // Joined
        void AppendValues(string key, params string[] values); // Raw
        void AppendCommaSeparatedValues(string key, params string[] values); // Joined + Quoting
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set", Justification = "Re-evaluate later.")]
        void Set(string key, string value); // raw (redundant with set values?)
        void SetValues(string key, params string[] values); // raw
        void SetCommaSeparatedValues(string key, params string[] values); // Joined + Quoting
    }
}
