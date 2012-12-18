using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Settings;
using Ninject;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Hosting.Tests
{
    public class AutofacContainerTests
    {
        [Fact]
        public void DefaultServicesCanBeResolved()
        {
            var builder = new ContainerBuilder();
            DefaultServices.ForEach((service, implementation) => builder.RegisterType(implementation).As(service));
            var container = builder.Build();

            container.Resolve<IKatanaEngine>().ShouldNotBe(null);
            container.Resolve<IKatanaSettingsProvider>().ShouldNotBe(null);
            container.Resolve<IAppBuilderFactory>().ShouldNotBe(null);
            container.Resolve<ITraceOutputBinder>().ShouldNotBe(null);
        }

        [Fact]
        public void MultipleLoadersCanBeRegistered()
        {
            var builder = new ContainerBuilder();
            DefaultServices.ForEach((service, implementation) => builder.RegisterType(implementation).As(service));
            builder.RegisterType<TestAppLoader1>().As<IAppLoader>();
            builder.RegisterType<TestAppLoader2>().As<IAppLoader>();
            var container = builder.Build();

            var loaderChain = container.Resolve<IAppLoaderChain>();
            loaderChain.Load("Hello").ShouldBe(TestAppLoader1.Result);
            loaderChain.Load("World").ShouldBe(TestAppLoader2.Result);
            loaderChain.Load("!").ShouldBe(null);
        }
    }
}
