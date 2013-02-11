// <copyright file="ServerFactoryAdapterTests.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

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

        [Fact]
        public void InitializeMethodIsCalledWithProperties()
        {
            var serverFactory = new InitializePatternTwo();
            var adapter = new ServerFactoryAdapter(serverFactory);
            IAppBuilder builder = new DefaultAppBuilderFactory().Create();
            adapter.Initialize(builder);
            builder.Properties["called"].ShouldBe(serverFactory);
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
