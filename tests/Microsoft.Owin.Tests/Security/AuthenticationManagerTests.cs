// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Owin.Tests.Security
{
    public class AuthenticationManagerTests
    {
        [Fact]
        public void NullUserReturnsNull()
        {
            IOwinContext context = new OwinContext();
            Assert.Null(context.Request.User);
            Assert.Null(context.Authentication.User);
        }
    }
}
