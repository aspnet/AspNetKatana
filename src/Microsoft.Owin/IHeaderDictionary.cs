// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin
{
    /// <summary>
    /// A wrapper for owin.RequestHeaders and owin.ResponseHeaders
    /// </summary>
    public interface IHeaderDictionary : IReadableStringCollection, IDictionary<string, string[]>
    {
        /// <summary>
        /// Get or set the associated header value in the collection.  Multiple values will be merged.
        /// Returns null if the key is not present.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        new string this[string key] { get; set; }

        /// <summary>
        /// Parses out comma separated headers into individual values.  Quoted values will not be coma split, and the quotes will be removed.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IList<string> GetCommaSeparatedValues(string key); // Split

        /// <summary>
        /// Add a new value. Appends to the header if already present
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Append(string key, string value); // Joined

        /// <summary>
        /// Add new values. Each item remains a separate array entry.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        void AppendValues(string key, params string[] values); // Raw

        /// <summary>
        /// Quotes any values containing comas, and then coma joins all of the values with any existing values.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        void AppendCommaSeparatedValues(string key, params string[] values); // Joined + Quoting

        /// <summary>
        /// Sets a specific header value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set", Justification = "Re-evaluate later.")]
        void Set(string key, string value); // raw (redundant with set values?)

        /// <summary>
        /// Sets the specified header values without modification
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        void SetValues(string key, params string[] values); // raw

        /// <summary>
        /// Quotes any values containing comas, and then coma joins all of the values.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        void SetCommaSeparatedValues(string key, params string[] values); // Joined + Quoting
    }
}
