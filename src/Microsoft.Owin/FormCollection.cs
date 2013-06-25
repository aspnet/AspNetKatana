// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Owin
{
    /// <summary>
    /// 
    /// </summary>
    public class FormCollection : ReadableStringCollection, IFormCollection
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="store"></param>
        public FormCollection(IDictionary<string, string[]> store)
            : base(store)
        {
        }
    }
}
