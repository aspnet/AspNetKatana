// -----------------------------------------------------------------------
// <copyright file="TimestampContentIdentifier.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Razor.Owin.IO;

namespace Microsoft.AspNet.Razor.Owin.Compilation
{
    public class TimestampContentIdentifier : IContentIdentifier
    {
        public string GenerateContentId(IFile file)
        {
            return String.Format("{0}@{1}", file.FullPath, file.LastModifiedTime.Ticks);
        }
    }
}
