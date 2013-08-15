// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using FakeN.Web;
using Shouldly;
using Xunit;

#if NET40
namespace Microsoft.Owin.Host.SystemWeb.Tests
#else

namespace Microsoft.Owin.Host.SystemWeb.Tests45
#endif
{
    public class OwinHttpHandlerTests : TestsBase
    {
        [Fact]
        public Task ItShouldCallAppDelegateWhenBeginProcessRequestCalled()
        {
            var httpHandler = new OwinHttpHandler(string.Empty, OwinBuilder.Build(WasCalledApp));
            FakeHttpContext httpContext = NewHttpContext(new Uri("http://localhost"));

            Task task = Task.Factory.FromAsync(httpHandler.BeginProcessRequest, httpHandler.EndProcessRequest, httpContext, null);
            return task.ContinueWith(_ =>
            {
                task.Exception.ShouldBe(null);
                WasCalled.ShouldBe(true);
            });
        }
    }
}
