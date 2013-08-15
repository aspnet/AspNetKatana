// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Starter;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Hosting.Tests.Containers
{
    public class ReplaceDefaultServiceWithTextFileTests
    {
        [Fact]
        public void ServiceFileWillOverrideServiceInterfacesAutomatically()
        {
            var services = new Dictionary<Type, Type>();
            ServicesFactory.ForEach(
                "Containers\\ReplaceDefaultServiceWithTextFileTests.txt",
                (service, implementation) => { services[service] = implementation; });
            services[typeof(IHostingStarterFactory)].ShouldBe(typeof(CustomStarterFactory));
        }

        public class CustomStarterFactory : IHostingStarterFactory
        {
            public IHostingStarter Create(string name)
            {
                return null;
            }
        }
    }
}
