// <copyright file="ReplaceDefaultServiceWithTextFileTests.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.Hosting.Services;
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
