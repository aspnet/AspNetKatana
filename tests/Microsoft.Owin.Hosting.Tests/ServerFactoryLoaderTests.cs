// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Builder;
using Microsoft.Owin.Host.HttpListener;
using Microsoft.Owin.Hosting.ServerFactory;
using Microsoft.Owin.Hosting.Services;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Hosting.Tests
{
    public class ServerFactoryLoaderTests
    {
        [Theory]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        [InlineData("Microsoft.Owin.Host.HttpListener.OwinServerFactory")]
        public void LoadWithDefaults_LoadAssemblyAndDiscoverFactory(string data)
        {
            var loader = new ServerFactoryLoader(new ServerFactoryActivator(ServicesFactory.Create()));
            IServerFactoryAdapter serverFactory = loader.Load(data);
            Assert.NotNull(serverFactory);
            IAppBuilder builder = new AppBuilder();
            serverFactory.Initialize(builder);
            Assert.IsType<OwinHttpListener>(builder.Properties[typeof(OwinHttpListener).FullName]);
        }

        [Fact]
        public void LoadWithAssemblyName_DiscoverDefaultFactoryName()
        {
            var loader = new ServerFactoryLoader(new ServerFactoryActivator(ServicesFactory.Create()));
            IServerFactoryAdapter serverFactory = loader.Load("Microsoft.Owin.Hosting.Tests");
            Assert.NotNull(serverFactory);
            IAppBuilder builder = new AppBuilder();
            serverFactory.Create(builder);
            Assert.Equal("Microsoft.Owin.Hosting.Tests.OwinServerFactory", builder.Properties["create.server"]);
        }

        [Theory]
        [InlineData("Microsoft.Owin.Hosting.Tests.OwinServerFactory")]
        [InlineData("Microsoft.Owin.Hosting.Tests.StaticServerFactory")]
        [InlineData("Microsoft.Owin.Hosting.Tests.InstanceServerFactory")]
        public void LoadWithAssemblyAndTypeName_Success(string data)
        {
            var loader = new ServerFactoryLoader(new ServerFactoryActivator(ServicesFactory.Create()));
            IServerFactoryAdapter serverFactory = loader.Load(data);
            Assert.NotNull(serverFactory);
            IAppBuilder builder = new AppBuilder();
            serverFactory.Create(builder);
            Assert.Equal(data, builder.Properties["create.server"]);
        }

        [Theory]
        [InlineData("Microsoft.Owin.Hosting.Tests.OwinServerFactory, Microsoft.Owin.Hosting.Tests, Culture=neutral, PublicKeyToken=null", "Microsoft.Owin.Hosting.Tests.OwinServerFactory")]
        [InlineData("Microsoft.Owin.Hosting.Tests.StaticServerFactory, Microsoft.Owin.Hosting.Tests, Culture=neutral, PublicKeyToken=null", "Microsoft.Owin.Hosting.Tests.StaticServerFactory")]
        [InlineData("Microsoft.Owin.Hosting.Tests.InstanceServerFactory, Microsoft.Owin.Hosting.Tests, Culture=neutral, PublicKeyToken=null", "Microsoft.Owin.Hosting.Tests.InstanceServerFactory")]
        public void LoadWithAssemblyAndFullTypeName_Success(string data, string expected)
        {
            var loader = new ServerFactoryLoader(new ServerFactoryActivator(ServicesFactory.Create()));
            IServerFactoryAdapter serverFactory = loader.Load(data);
            Assert.NotNull(serverFactory);
            IAppBuilder builder = new AppBuilder();
            serverFactory.Create(builder);
            Assert.Equal(expected, builder.Properties["create.server"]);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("OwinServerFactory")]
        [InlineData("Microsoft.Owin")]
        [InlineData("Microsoft.Owin.Hosting")]
        [InlineData("Microsoft.Owin.Hosting.Tests.MissingServerFactory")]
        [InlineData("Microsoft.Owin.Hosting.Tests.Nested.MissingServerFactory")]
        public void LoadWithWrongAssemblyOrType_ReturnsNull(string data)
        {
            var loader = new ServerFactoryLoader(new ServerFactoryActivator(ServicesFactory.Create()));
            IServerFactoryAdapter serverFactory = loader.Load(data);
            Assert.Null(serverFactory);
        }
    }
}
