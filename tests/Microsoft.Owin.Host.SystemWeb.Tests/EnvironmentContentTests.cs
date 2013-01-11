// <copyright file="EnvironmentContentTests.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Shouldly;

using Xunit;

#if NET40
namespace Microsoft.Owin.Host.SystemWeb.Tests
#else
namespace Microsoft.Owin.Host.SystemWeb.Tests45
#endif
{
    using AppDelegate = Func<IDictionary<string, object>, Task>;

    public class EnvironmentContentTests : TestsBase
    {
        [Fact]
        public Task ItShouldContainRequestContextAndAnHttpContextBaseWhenCalledThroughRoute()
        {
            var routes = new RouteCollection();
            routes.MapOwinPath<AppDelegate>(string.Empty, WasCalledApp);
            RequestContext requestContext = NewRequestContext(routes, NewHttpContext(new Uri("http://localhost")));

            Task task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(
                _ =>
                {
                    task.Exception.ShouldBe(null);
                    WasCalled.ShouldBe(true);
                    WasCalledInput.ShouldContainKeyAndValue(typeof(RequestContext).FullName, requestContext);
                    WasCalledInput.ShouldContainKey(typeof(HttpContextBase).FullName);
                });
        }

        [Fact]
        public Task ItShouldContainAllOwinStandardKeys()
        {
            var routes = new RouteCollection();
            routes.MapOwinPath<AppDelegate>(string.Empty, WasCalledApp);
            RequestContext requestContext = NewRequestContext(routes, NewHttpContext(new Uri("http://localhost")));

            Task task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(
                _ =>
                {
                    task.Exception.ShouldBe(null);
                    WasCalled.ShouldBe(true);
                    WasCalledInput.ShouldContainKey("owin.RequestMethod");
                    WasCalledInput.ShouldContainKey("owin.RequestPath");
                    WasCalledInput.ShouldContainKey("owin.RequestPathBase");
                    WasCalledInput.ShouldContainKey("owin.RequestQueryString");
                    WasCalledInput.ShouldContainKey("owin.RequestScheme");
                    WasCalledInput.ShouldContainKey("owin.Version");
                });
        }

        [Fact]
        public Task ItShouldContainGivenRequestMethod()
        {
            var routes = new RouteCollection();
            routes.MapOwinPath<AppDelegate>(string.Empty, WasCalledApp);
            RequestContext requestContext = NewRequestContext(routes, NewHttpContext(new Uri("http://localhost"), "DELTA"));

            Task task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(
                _ =>
                {
                    task.Exception.ShouldBe(null);
                    WasCalled.ShouldBe(true);
                    WasCalledInput.ShouldContainKeyAndValue("owin.RequestMethod", "DELTA");
                });
        }

        [Fact]
        public Task ItShouldHaveEmptyPathBaseAndAbsolutePath()
        {
            var routes = new RouteCollection();
            routes.MapOwinPath<AppDelegate>(string.Empty, WasCalledApp);
            RequestContext requestContext = NewRequestContext(routes, NewHttpContext(new Uri("http://localhost/alpha/beta")));

            Task task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(
                _ =>
                {
                    task.Exception.ShouldBe(null);
                    WasCalled.ShouldBe(true);
                    WasCalledInput.ShouldContainKeyAndValue("owin.RequestPathBase", string.Empty);
                    WasCalledInput.ShouldContainKeyAndValue("owin.RequestPath", "/alpha/beta");
                });
        }

        [Fact]
        public Task ItShouldHaveUnparsedAndEscapedQueryString()
        {
            var routes = new RouteCollection();
            routes.MapOwinPath<AppDelegate>(string.Empty, WasCalledApp);
            RequestContext requestContext = NewRequestContext(routes, NewHttpContext(new Uri("http://localhost/alpha/beta?gamma=delta&omega=%2fepsilon")));

            Task task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(
                _ =>
                {
                    task.Exception.ShouldBe(null);
                    WasCalled.ShouldBe(true);
                    WasCalledInput.ShouldContainKeyAndValue("owin.RequestQueryString", "gamma=delta&omega=%2fepsilon");
                });
        }

        [Fact]
        public Task ItShouldFireOnSendingHeaders()
        {
            var stateObject = new object();
            bool onSendingHeadersFired = false;
            bool stateObjectMatched = false;

            var routes = new RouteCollection();
            routes.MapOwinPath<AppDelegate>(string.Empty,
                env =>
                {
                    var onSendingHeadersRegister = env.Get<Action<Action<object>, object>>("server.OnSendingHeaders");
                    onSendingHeadersRegister(
                        passedObject =>
                        {
                            onSendingHeadersFired = true;
                            stateObjectMatched = object.ReferenceEquals(passedObject, stateObject);
                        }, stateObject);
                    return TaskHelpers.Completed();
                });
            RequestContext requestContext = NewRequestContext(routes, NewHttpContext(new Uri("http://localhost/alpha/beta")));

            Task task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(
                _ =>
                {
                    task.Exception.ShouldBe(null);
                    onSendingHeadersFired.ShouldBe(true);
                    stateObjectMatched.ShouldBe(true);
                });
        }
    }
}
