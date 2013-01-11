// -----------------------------------------------------------------------
// <copyright file="IContentIdentifier.cs" company="Microsoft">
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
    public interface IContentIdentifier
    {
        string GenerateContentId(IFile file);
    }
}
