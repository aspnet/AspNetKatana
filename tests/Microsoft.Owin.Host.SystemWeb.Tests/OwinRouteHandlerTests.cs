// <copyright file="OwinRouteHandlerTests.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Web;
using System.Web.Routing;
using FakeN.Web;
using Shouldly;
using Xunit;

#if NET40
namespace Microsoft.Owin.Host.SystemWeb.Tests
#else

namespace Microsoft.Owin.Host.SystemWeb.Tests45
#endif
{
    public class OwinRouteHandlerTests : TestsBase
    {
        [Fact]
        public void ItShouldReturnAnOwinHttpHandler()
        {
            FakeHttpContext httpContext = NewHttpContext(new Uri("http://localhost"));
            RequestContext requestContext = NewRequestContext(new OwinRoute(string.Empty, () => null), httpContext);

            IHttpHandler httpHandler = requestContext.RouteData.RouteHandler.GetHttpHandler(requestContext);

            requestContext.RouteData.RouteHandler.ShouldBeTypeOf<OwinRouteHandler>();
            httpHandler.ShouldNotBe(null);
            httpHandler.ShouldBeTypeOf<OwinHttpHandler>();
        }
    }
}
