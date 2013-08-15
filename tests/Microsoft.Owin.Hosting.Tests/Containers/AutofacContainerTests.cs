// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Autofac;
using Microsoft.Owin.Hosting.Loader;
using Microsoft.Owin.Hosting.Services;

namespace Microsoft.Owin.Hosting.Tests.Containers
{
    public class AutofacContainerTests : ContainerTestsBase
    {
        public override Func<Type, object> CreateContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AutofacServiceProvider>().As<IServiceProvider>();
            ServicesFactory.ForEach((service, implementation) => builder.RegisterType(implementation).As(service));
            builder.RegisterType<TestAppLoader1>().As<IAppLoaderFactory>();
            builder.RegisterType<TestAppLoader2>().As<IAppLoaderFactory>();
            IContainer container = builder.Build();
            return container.Resolve;
        }

        public class AutofacServiceProvider : IServiceProvider
        {
            private readonly IComponentContext _componentContext;

            public AutofacServiceProvider(IComponentContext componentContext)
            {
                _componentContext = componentContext;
            }

            public object GetService(Type serviceType)
            {
                return _componentContext.Resolve(serviceType);
            }
        }
    }
}
