// <copyright file="CastleWindsorContainerTests.cs" company="Katana contributors">
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
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Microsoft.Owin.Hosting.Loader;
using Microsoft.Owin.Hosting.Services;

namespace Microsoft.Owin.Hosting.Tests.Containers
{
    public class CastleWindsorContainerTests : ContainerTestsBase
    {
        public override Func<Type, object> CreateContainer()
        {
            var container = new WindsorContainer();

            container.Kernel.Resolver.AddSubResolver(
                new CollectionResolver(container.Kernel, true));

            container.Register(
                Component.For<IServiceProvider>().ImplementedBy<WindsorServiceProvider>());

            DefaultServices.ForEach((service, implementation) =>
                container.Register(Component.For(service).ImplementedBy(implementation)));

            container.Register(
                Component.For<IAppLoaderProvider>().ImplementedBy<TestAppLoader1>(),
                Component.For<IAppLoaderProvider>().ImplementedBy<TestAppLoader2>());

            return container.Resolve;
        }

        public class WindsorServiceProvider : IServiceProvider
        {
            private readonly IKernel _kernel;

            public WindsorServiceProvider(IKernel kernel)
            {
                _kernel = kernel;
            }

            public object GetService(Type serviceType)
            {
                return _kernel.Resolve(serviceType);
            }
        }
    }
}
