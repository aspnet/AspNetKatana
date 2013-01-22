// <copyright file="NinjectContainerTests.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
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
using Microsoft.Owin.Hosting.Loader;
using Microsoft.Owin.Hosting.Services;
using Ninject;

namespace Microsoft.Owin.Hosting.Tests.Containers
{
    public class NinjectContainerTests : ContainerTestsBase
    {
        public override Func<Type, object> CreateContainer()
        {
            var kernel = new StandardKernel();
            kernel.Bind<IServiceProvider>().To<NinjectServiceProvider>();
            DefaultServices.ForEach((service, implementation) => kernel.Bind(service).To(implementation));
            kernel.Bind<IAppLoaderProvider>().To<TestAppLoader1>();
            kernel.Bind<IAppLoaderProvider>().To<TestAppLoader2>();
            return serviceType => kernel.Get(serviceType);
        }

        public class NinjectServiceProvider : IServiceProvider
        {
            private readonly IKernel _kernel;

            public NinjectServiceProvider(IKernel kernel)
            {
                _kernel = kernel;
            }

            public object GetService(Type serviceType)
            {
                return _kernel.Get(serviceType);
            }
        }
    }
}
