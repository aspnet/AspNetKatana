// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
            ServicesFactory.ForEach((service, implementation) =>
            {
                if (service == typeof(IAppLoaderFactory))
                {
                    container.Register(service, implementation, implementation.FullName);
                }
                else
                {
                    container.Register(service, implementation);
                }
            });
            container.Register<IAppLoaderFactory, TestAppLoader1>("1");
            container.Register<IAppLoaderFactory, TestAppLoader2>("2");
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
