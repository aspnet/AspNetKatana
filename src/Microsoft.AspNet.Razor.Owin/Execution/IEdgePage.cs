// -----------------------------------------------------------------------
// <copyright file="IRazorPage.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gate;

namespace Microsoft.AspNet.Razor.Owin.Execution
{
    public interface IRazorPage
    {
        Task Run(Request req, Response resp);
    }
}
