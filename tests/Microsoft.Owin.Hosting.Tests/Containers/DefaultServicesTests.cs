using System;
using Microsoft.Owin.Hosting.Loader;
using Microsoft.Owin.Hosting.Services;

namespace Microsoft.Owin.Hosting.Tests.Containers
{
    public class DefaultServicesTests : ContainerTestsBase
    {
        public override Func<Type, object> CreateContainer()
        {
            var services = DefaultServices.Create(reg => reg
                .Add<IAppLoaderProvider, TestAppLoader1>()
                .Add<IAppLoaderProvider, TestAppLoader2>());
            return services.GetService;
        }
    }
}
