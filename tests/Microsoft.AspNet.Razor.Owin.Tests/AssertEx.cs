// -----------------------------------------------------------------------
// <copyright file="AssertEx.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.Razor.Owin.Tests
{
    public static class AssertEx
    {
        public static async Task<TException> Throws<TException>(Func<Task> action) where TException : Exception
        {
            TException thrown = null;
            try
            {
                await action();
            }
            catch (TException ex)
            {
                thrown = ex;
            }
            Assert.NotNull(thrown);
            return thrown;
        }
    }
}
