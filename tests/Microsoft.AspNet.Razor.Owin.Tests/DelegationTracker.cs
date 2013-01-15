// -----------------------------------------------------------------------
// <copyright file="DelegationTracker.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin;

namespace Microsoft.AspNet.Razor.Owin.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class DelegationTracker
    {
        public DelegationTracker()
        {
            Next = cp =>
            {
                NextCallParams = cp;
                NextWasCalled = true;
                return Task.FromResult<object>(null);
            };
        }

        public bool NextWasCalled { get; private set; }
        public IDictionary<string, object> NextCallParams { get; private set; }
        public AppFunc Next { get; private set; }
    }
}
