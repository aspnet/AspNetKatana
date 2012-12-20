using System;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Settings;
using Microsoft.Owin.Hosting.Starter;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Hosting.Tests.Containers
{
    public abstract class ContainerTestsBase
    {
        public abstract Func<Type, object> CreateContainer();

        [Fact]
        public void DefaultServicesCanBeResolved()
        {
            var container = CreateContainer();

            container(typeof(ITraceOutputBinder)).ShouldNotBe(null);
            container(typeof(IKatanaStarter)).ShouldNotBe(null);
            container(typeof(IKatanaEngine)).ShouldNotBe(null);
            container(typeof(IKatanaSettingsProvider)).ShouldNotBe(null);
            container(typeof(IAppBuilderFactory)).ShouldNotBe(null);

            DefaultServices.ForEach(
                (service, implementation) =>
                {
                    if (service != typeof(IAppLoader))
                    {
                        container(service).ShouldNotBe(null);
                    }
                });
        }

        [Fact]
        public void MultipleLoadersCanBeRegistered()
        {
            var container = CreateContainer();

            var loaderChain = (IAppLoaderChain)container(typeof(IAppLoaderChain));
            loaderChain.Load("Hello").ShouldBe(TestAppLoader1.Result);
            loaderChain.Load("World").ShouldBe(TestAppLoader2.Result);
            loaderChain.Load("!").ShouldBe(null);
        }

        [Fact]
        public void NamedStarterCanBeResolved()
        {
            var container = CreateContainer();
            var hostingStarterFactory = (IHostingStarterFactory)container(typeof(IHostingStarterFactory));
            var hostingStarter = hostingStarterFactory.Create("Microsoft.Owin.Hosting.Tests");
            hostingStarter.ShouldBeTypeOf<TestHostingStarter>();
        }
    }
}