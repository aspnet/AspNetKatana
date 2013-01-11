// -----------------------------------------------------------------------
// <copyright file="ActivationResult.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.AspNet.Razor.Owin.Execution
{
    public class ActivationResult
    {
        private ActivationResult(bool success, IRazorPage page)
        {
            Success = success;
            Page = page;
        }

        public bool Success { get; private set; }
        public IRazorPage Page { get; private set; }

        public static ActivationResult Failed()
        {
            return new ActivationResult(false, null);
        }

        public static ActivationResult Successful(IRazorPage page)
        {
            return new ActivationResult(true, page);
        }
    }
}
