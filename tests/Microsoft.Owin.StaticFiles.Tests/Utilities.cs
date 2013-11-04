// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Xunit;

namespace Microsoft.Owin.StaticFiles.Tests
{
    internal class Utilities
    {
        internal static void Throws<TException>(Action action) where TException : Exception
        {
            try
            {
                action();
                Assert.False(true, "No Exception");
            }
            catch (TargetInvocationException tex)
            {
                Assert.IsType<TException>(tex.InnerException);
            }
            catch (TException)
            {
            }
        }
    }
}
