using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Settings;
using Ninject;
using Ninject.Modules;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Hosting.Tests
{
    public class NinjectContainerTests
    {
        [Fact]
        public void DefaultServicesCanBeResolved()
        {
            var kernel = new StandardKernel(new KatanaModule());
            kernel.GetService<IKatanaEngine>().ShouldNotBe(null);
            kernel.GetService<IKatanaSettingsProvider>().ShouldNotBe(null);
            kernel.GetService<IAppBuilderFactory>().ShouldNotBe(null);
            kernel.GetService<ITraceOutputBinder>().ShouldNotBe(null);
        }

        [Fact]
        public void MultipleLoadersCanBeRegistered()
        {
            var kernel = new StandardKernel(new KatanaModule());
            var loaderChain = kernel.Get<IAppLoaderChain>();
            loaderChain.Load("Hello").ShouldBe(TestAppLoader1.Result);
            loaderChain.Load("World").ShouldBe(TestAppLoader2.Result);
            loaderChain.Load("!").ShouldBe(null);
        }

        public class KatanaModule : NinjectModule
        {
            public override void Load()
            {
                DefaultServices.ForEach((service, implementation) => Bind(service).To(implementation));
                Bind<IAppLoader>().To<TestAppLoader1>();
                Bind<IAppLoader>().To<TestAppLoader2>();
            }

            public class Adder : DefaultServices.IServiceAdder
            {
                private readonly KatanaModule _katanaModule;

                public Adder(KatanaModule katanaModule)
                {
                    _katanaModule = katanaModule;
                }

                public void Add<TService, TClass>() where TClass : TService
                {
                    _katanaModule.Bind<TService>().To<TClass>();
                }
            }
        }
    }
}
