// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Owin.Hosting.Builder;
using Microsoft.Owin.Hosting.Engine;
using Microsoft.Owin.Hosting.Loader;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Starter;
using Microsoft.Owin.Hosting.Tracing;
using Microsoft.Owin.Logging;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Hosting.Tests.Containers
{
    public abstract class ContainerTestsBase
    {
        public abstract Func<Type, object> CreateContainer();

        [Fact]
        public void DefaultServicesCanBeResolved()
        {
            Func<Type, object> container = CreateContainer();

            container(typeof(ITraceOutputFactory)).ShouldNotBe(null);
            container(typeof(IHostingStarter)).ShouldNotBe(null);
            container(typeof(ILoggerFactory)).ShouldNotBe(null);
            container(typeof(IHostingEngine)).ShouldNotBe(null);
            container(typeof(IAppBuilderFactory)).ShouldNotBe(null);

            ServicesFactory.ForEach(
                (service, implementation) =>
                {
                    if (service != typeof(IAppLoaderFactory))
                    {
                        container(service).ShouldNotBe(null);
                    }
                });
        }

        [Fact]
        public void MultipleLoadersCanBeRegistered()
        {
            Func<Type, object> container = CreateContainer();

            IList<string> errors = new List<string>();
            var loaderChain = (IAppLoader)container(typeof(IAppLoader));
            loaderChain.Load("Hello", errors).ShouldBe(TestAppLoader1.Result);
            loaderChain.Load("World", errors).ShouldBe(TestAppLoader2.Result);
            Assert.NotEmpty(errors);
            errors.Clear();
            loaderChain.Load("!", errors).ShouldBe(null);
            Assert.NotEmpty(errors);
        }

        [Fact]
        public void NamedStarterCanBeResolved()
        {
            Func<Type, object> container = CreateContainer();
            var hostingStarterFactory = (IHostingStarterFactory)container(typeof(IHostingStarterFactory));
            IHostingStarter hostingStarter = hostingStarterFactory.Create("Microsoft.Owin.Hosting.Tests");
            hostingStarter.ShouldBeTypeOf<TestHostingStarter>();
        }
    }
}
