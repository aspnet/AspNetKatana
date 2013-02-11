// <copyright file="ContainerTestsBase.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.Hosting.Builder;
using Microsoft.Owin.Hosting.Loader;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Starter;
using Microsoft.Owin.Hosting.Tracing;
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

            container(typeof(ITraceOutputBinder)).ShouldNotBe(null);
            container(typeof(IKatanaStarter)).ShouldNotBe(null);
            container(typeof(IKatanaEngine)).ShouldNotBe(null);
            container(typeof(IAppBuilderFactory)).ShouldNotBe(null);

            DefaultServices.ForEach(
                (service, implementation) =>
                {
                    if (service != typeof(IAppLoaderProvider))
                    {
                        container(service).ShouldNotBe(null);
                    }
                });
        }

        [Fact]
        public void MultipleLoadersCanBeRegistered()
        {
            Func<Type, object> container = CreateContainer();

            var loaderChain = (IAppLoaderManager)container(typeof(IAppLoaderManager));
            loaderChain.Load("Hello").ShouldBe(TestAppLoader1.Result);
            loaderChain.Load("World").ShouldBe(TestAppLoader2.Result);
            loaderChain.Load("!").ShouldBe(null);
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
