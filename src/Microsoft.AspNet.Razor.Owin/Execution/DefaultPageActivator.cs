// -----------------------------------------------------------------------
// <copyright file="DefaultPageActivator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.Razor.Owin;

namespace Microsoft.AspNet.Razor.Owin.Execution
{
    public class DefaultPageActivator : IPageActivator
    {
        public ActivationResult ActivatePage(Type type, ITrace tracer)
        {
            Requires.NotNull(type, "type");
            Requires.NotNull(tracer, "tracer");

            IRazorPage page = null;
            try
            {
                page = Activator.CreateInstance(type) as IRazorPage;
            }
            catch (MissingMethodException)
            {
                return ActivationResult.Failed();
            }

            if (page == null)
            {
                return ActivationResult.Failed();
            }
            else
            {
                return ActivationResult.Successful(page);
            }
        }
    }
}
