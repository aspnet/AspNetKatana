// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
            IAppBuilder builder = new AppBuilderFactory().Create();
            adapter.Initialize(builder);
            builder.Properties["called"].ShouldBe(serverFactory);
        }

        [Fact]
        public void InitializeMethodIsCalledWithProperties()
        {
            var serverFactory = new InitializePatternTwo();
            var adapter = new ServerFactoryAdapter(serverFactory);
            IAppBuilder builder = new AppBuilderFactory().Create();
            adapter.Initialize(builder);
            builder.Properties["called"].ShouldBe(serverFactory);
        }

        [Fact]
        public void CreateMethodCalledWithAppAndProperties()
        {
            var serverFactory = new CreatePatternOne();
            var adapter = new ServerFactoryAdapter(serverFactory);
            IAppBuilder builder = new AppBuilderFactory().Create();
            IDisposable disposable = adapter.Create(builder);
            builder.Properties["called"].ShouldBe(serverFactory);
            builder.Properties["app"].ShouldNotBe(null);
            builder.Properties["properties"].ShouldBeSameAs(builder.Properties);
            disposable.ShouldBe(serverFactory);
        }

        public class InitializePatternOne
        {
            public void Initialize(IAppBuilder builder)
            {
                builder.Properties["called"] = this;
            }
        }

        public class InitializePatternTwo
        {
            public void Initialize(IDictionary<string, object> properties)
            {
                properties["called"] = this;
            }
        }

        public sealed class CreatePatternOne : IDisposable
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
