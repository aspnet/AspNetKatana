// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Katana.Performance.ReferenceApp
{
    public class CanonicalRequestAttribute : Attribute
    {
        public string Path { get; set; }
        public string Description { get; set; }
    }
}
