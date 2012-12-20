using System;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
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
                Component.For<IAppLoader>().ImplementedBy<TestAppLoader1>(),
                Component.For<IAppLoader>().ImplementedBy<TestAppLoader2>());

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
