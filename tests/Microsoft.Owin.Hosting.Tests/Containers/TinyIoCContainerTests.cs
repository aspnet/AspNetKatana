// <copyright file="TinyIoCContainerTests.cs" company="Katana contributors">
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
using TinyIoC;

namespace Microsoft.Owin.Hosting.Tests.Containers
{
    public class TinyIoCContainerTests : ContainerTestsBase
    {
        public override Func<Type, object> CreateContainer()
        {
            var container = new TinyIoCContainer();
            container.Register<IServiceProvider, TinyIoCServiceProvider>();
            DefaultServices.ForEach((service, implementation) =>
            {
                if (service == typeof(IAppLoaderProvider))
                {
                    container.Register(service, implementation, implementation.FullName);
                }
                else
                {
                    container.Register(service, implementation);
                }
            });
            container.Register<IAppLoaderProvider, TestAppLoader1>("1");
            container.Register<IAppLoaderProvider, TestAppLoader2>("2");
            return container.Resolve;
        }

        public class TinyIoCServiceProvider : IServiceProvider
        {
            private readonly TinyIoCContainer _container;

            public TinyIoCServiceProvider(TinyIoCContainer container)
            {
                _container = container;
            }

            public object GetService(Type serviceType)
            {
                return _container.Resolve(serviceType);
            }
        }
    }
}
