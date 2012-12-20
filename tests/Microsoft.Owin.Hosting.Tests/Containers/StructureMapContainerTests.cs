using System;
using Microsoft.Owin.Hosting.Loader;
using Microsoft.Owin.Hosting.Services;
using StructureMap;

namespace Microsoft.Owin.Hosting.Tests.Containers
{
    public class StructureMapContainerTests : ContainerTestsBase
    {
        public override Func<Type, object> CreateContainer()
        {
            var container = new Container(config =>
            {
                DefaultServices.ForEach((service, implementation) =>
                    config.For(service).Use(implementation));
                config.For<IAppLoaderProvider>().Use<TestAppLoader1>();
                config.For<IAppLoaderProvider>().Use<TestAppLoader2>();
                config.For<IServiceProvider>().Use<StructureMapServiceProvider>();
            });
            return container.GetInstance;
        }

        public class StructureMapServiceProvider : IServiceProvider
        {
            private readonly IContainer _container;

            public StructureMapServiceProvider(IContainer container)
            {
                _container = container;
            }

            public object GetService(Type serviceType)
            {
                return _container.GetInstance(serviceType);
            }
        }
    }
}
