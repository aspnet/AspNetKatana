using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Settings;
using Shouldly;
using StructureMap;
using Xunit;

namespace Microsoft.Owin.Hosting.Tests
{
    public class StructureMapContainerTests
    {
        [Fact]
        public void DefaultServicesCanBeResolved()
        {
            var container = new Container(config =>
                DefaultServices.ForEach((service, implementation) =>
                    config.For(service).Use(implementation)));

            container.GetInstance<IKatanaEngine>().ShouldNotBe(null);
            container.GetInstance<IKatanaSettingsProvider>().ShouldNotBe(null);
            container.GetInstance<IAppBuilderFactory>().ShouldNotBe(null);
            container.GetInstance<ITraceOutputBinder>().ShouldNotBe(null);
        }

        [Fact]
        public void MultipleLoadersCanBeRegistered()
        {
            var container = new Container(config =>
            {
                DefaultServices.ForEach((service, implementation) =>
                    config.For(service).Use(implementation));
                config.For<IAppLoader>().Use<TestAppLoader1>();
                config.For<IAppLoader>().Use<TestAppLoader2>();
            });


            var loaderChain = container.GetInstance<IAppLoaderChain>();
            loaderChain.Load("Hello").ShouldBe(TestAppLoader1.Result);
            loaderChain.Load("World").ShouldBe(TestAppLoader2.Result);
            loaderChain.Load("!").ShouldBe(null);
        }
    }
}
