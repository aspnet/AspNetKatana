// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.StaticFiles
{
    [Flags]
    public enum HeaderFields
    {
        None = 0,
        LastModified = 1,
        ETag = 2,
        Expires = 4,
        CacheControl = 8,
    }
}
