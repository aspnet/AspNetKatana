// -----------------------------------------------------------------------
// <copyright file="IPageActivator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.AspNet.Razor.Owin.Execution
{
    public interface IPageActivator
    {
        ActivationResult ActivatePage(Type type, ITrace tracer);
    }
}
