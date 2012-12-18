using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Settings;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Hosting.Tests
{
    public class DefaultServicesTests
    {
        [Fact]
        public void DefaultServicesCanBeResolved()
        {
            var provider = DefaultServices.Create();
            provider.GetService<IKatanaEngine>().ShouldNotBe(null);
            provider.GetService<IKatanaSettingsProvider>().ShouldNotBe(null);
            provider.GetService<IAppBuilderFactory>().ShouldNotBe(null);
            provider.GetService<ITraceOutputBinder>().ShouldNotBe(null);
        }

        [Fact]
        public void MultipleLoadersCanBeRegistered()
        {
            var services = DefaultServices.Create(reg=>reg
                .Add<IAppLoader>(()=>new TestAppLoader1())
                .Add<IAppLoader>(() => new TestAppLoader2()));
            var loaderChain = services.GetService<IAppLoaderChain>();
            loaderChain.Load("Hello").ShouldBe(TestAppLoader1.Result);
            loaderChain.Load("World").ShouldBe(TestAppLoader2.Result);
            loaderChain.Load("!").ShouldBe(null);
        }
    }
}
