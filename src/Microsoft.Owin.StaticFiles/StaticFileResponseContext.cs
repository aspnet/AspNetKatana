// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.FileSystems;

namespace Microsoft.Owin.StaticFiles
{
    public class StaticFileResponseContext
    {
        public IOwinContext OwinContext { get; internal set; }
        public IFileInfo File { get; internal set; }
    }
}
