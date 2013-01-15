// -----------------------------------------------------------------------
// <copyright file="IErrorMessage.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.Razor.Owin.Compilation;

namespace Microsoft.AspNet.Razor.Owin
{
    public interface IErrorMessage
    {
        FileLocation Location { get; }
        string Message { get; }
    }
}
