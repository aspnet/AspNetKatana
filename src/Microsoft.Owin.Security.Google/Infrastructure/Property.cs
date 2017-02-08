// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.Google.Infrastructure
{
    internal class Property
    {
        public string Key { get; set; }
        public string Namespace { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
