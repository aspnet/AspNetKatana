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
