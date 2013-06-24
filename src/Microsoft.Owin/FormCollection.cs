// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Owin
{
    public class FormCollection : ReadableStringCollection, IFormCollection
    {
        public FormCollection(IDictionary<string, string[]> store)
            : base(store)
        {
        }
    }
}
