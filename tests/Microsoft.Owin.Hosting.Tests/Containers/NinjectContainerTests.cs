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
