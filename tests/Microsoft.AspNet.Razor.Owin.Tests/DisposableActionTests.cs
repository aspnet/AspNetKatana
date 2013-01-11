// -----------------------------------------------------------------------
// <copyright file="DisposableActionTests.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Razor.Owin;
using Xunit;

namespace Microsoft.AspNet.Razor.Owin.Tests
{
    public class DisposableActionTests
    {
        public class TheConstructor
        {
            [Fact]
            public void RequiresNonNullAction()
            {
                ContractAssert.NotNull(() => new DisposableAction(null), "act");
            }
        }

        public class TheDisposeMethod
        {
            [Fact]
            public void InvokesTheAction()
            {
                bool invoked = false;
                new DisposableAction(() => invoked = true).Dispose();
                Assert.True(invoked);
            }
        }
    }
}
