using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Settings;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Hosting.Tests
{
    public class CastleWindsorContainerTests
    {
        [Fact]
        public void DefaultServicesCanBeResolved()
        {
            var container = new WindsorContainer();
            container.Kernel.Resolver.AddSubResolver(
                new CollectionResolver(container.Kernel, true));

            DefaultServices.ForEach((service, implementation) =>
                container.Register(Component.For(service).ImplementedBy(implementation)));

            container.Resolve<IKatanaEngine>().ShouldNotBe(null);
            container.Resolve<IKatanaSettingsProvider>().ShouldNotBe(null);
            container.Resolve<IAppBuilderFactory>().ShouldNotBe(null);
            container.Resolve<ITraceOutputBinder>().ShouldNotBe(null);
        }

        [Fact]
        public void MultipleLoadersCanBeRegistered()
        {
            var container = new WindsorContainer();
            container.Kernel.Resolver.AddSubResolver(
                new CollectionResolver(container.Kernel, true));

            DefaultServices.ForEach((service, implementation) =>
                container.Register(Component.For(service).ImplementedBy(implementation)));

            container.Register(
                Component.For<IAppLoader>().ImplementedBy<TestAppLoader1>(),
                Component.For<IAppLoader>().ImplementedBy<TestAppLoader2>());

            var loaderChain = container.Resolve<IAppLoaderChain>();
            loaderChain.Load("Hello").ShouldBe(TestAppLoader1.Result);
            loaderChain.Load("World").ShouldBe(TestAppLoader2.Result);
            loaderChain.Load("!").ShouldBe(null);
        }
    }
}
