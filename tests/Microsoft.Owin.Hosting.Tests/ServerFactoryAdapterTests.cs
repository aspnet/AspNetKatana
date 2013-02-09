using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting.Builder;
using Microsoft.Owin.Hosting.ServerFactory;
using Owin;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Hosting.Tests
{
    public class ServerFactoryAdapterTests
    {
        [Fact]
        public void InitializeMethodIsCalledWithBuilder()
        {
            var serverFactory = new InitializePatternOne();
            var adapter = new ServerFactoryAdapter(serverFactory);
            IAppBuilder builder = new DefaultAppBuilderFactory().Create();
            adapter.Initialize(builder);
            builder.Properties["called"].ShouldBe(serverFactory);
        }

        public class InitializePatternOne
        {
            public void Initialize(IAppBuilder builder)
            {
                builder.Properties["called"] = this;
            }
        }

        [Fact]
        public void InitializeMethodIsCalledWithProperties()
        {
            var serverFactory = new InitializePatternTwo();
            var adapter = new ServerFactoryAdapter(serverFactory);
            IAppBuilder builder = new DefaultAppBuilderFactory().Create();
            adapter.Initialize(builder);
            builder.Properties["called"].ShouldBe(serverFactory);
        }

        public class InitializePatternTwo
        {
            public void Initialize(IDictionary<string, object> properties)
            {
                properties["called"] = this;
            }
        }

        [Fact]
        public void CreateMethodCalledWithAppAndProperties()
        {
            var serverFactory = new CreatePatternOne();
            var adapter = new ServerFactoryAdapter(serverFactory);
            IAppBuilder builder = new DefaultAppBuilderFactory().Create();
            IDisposable disposable = adapter.Create(builder);
            builder.Properties["called"].ShouldBe(serverFactory);
            builder.Properties["app"].ShouldNotBe(null);
            builder.Properties["properties"].ShouldBeSameAs(builder.Properties);
            disposable.ShouldBe(serverFactory);
        }

        public class CreatePatternOne : IDisposable
        {
            public IDisposable Create(Func<IDictionary<string, object>, Task> app, IDictionary<string, object> properties)
            {
                properties["called"] = this;
                properties["app"] = app;
                properties["properties"] = properties;
                return this;
            }

            public void Dispose()
            {
                
            }
        }
    }
}
