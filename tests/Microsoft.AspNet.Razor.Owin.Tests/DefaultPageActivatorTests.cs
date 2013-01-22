// <copyright file="DefaultPageActivatorTests.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
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

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Razor.Owin.Execution;
using Xunit;

namespace Microsoft.AspNet.Razor.Owin.Tests
{
    public class DefaultPageActivatorTests
    {
        public class TheActivatePageMethod
        {
            [Fact]
            public void RequiresNonNullType()
            {
                ContractAssert.NotNull(() => new DefaultPageActivator().ActivatePage(null, NullTrace.Instance), "type");
            }

            [Fact]
            public void RequiresNonNullTracer()
            {
                ContractAssert.NotNull(() => new DefaultPageActivator().ActivatePage(typeof(object), null), "tracer");
            }

            [Fact]
            public void ReturnsSuccessfulResultIfTypeIsPubliclyConstructableEdgePage()
            {
                // Arrange
                var activator = new DefaultPageActivator();

                // Act
                ActivationResult result = activator.ActivatePage(typeof(ConstructableEdgePage), NullTrace.Instance);

                // Assert
                Assert.True(result.Success);
                Assert.IsType<ConstructableEdgePage>(result.Page);
            }

            [Fact]
            public void ReturnsFailedResultIfTypeIsEdgePageButNotPubliclyConstructable()
            {
                // Arrange
                var activator = new DefaultPageActivator();

                // Act
                ActivationResult result = activator.ActivatePage(typeof(NonConstructableEdgePage), NullTrace.Instance);

                // Assert
                Assert.False(result.Success);
                Assert.Null(result.Page);
            }

            [Fact]
            public void ReturnsFailedResultIfTypeIsEdgePageButHasNoPublicParameterlessCtor()
            {
                // Arrange
                var activator = new DefaultPageActivator();

                // Act
                ActivationResult result = activator.ActivatePage(typeof(NoParameterlessConstructorEdgePage), NullTrace.Instance);

                // Assert
                Assert.False(result.Success);
                Assert.Null(result.Page);
            }

            [Fact]
            public void ReturnsFailedResultIfTypeIsConstructableButNotEdgePage()
            {
                // Arrange
                var activator = new DefaultPageActivator();

                // Act
                ActivationResult result = activator.ActivatePage(typeof(object), NullTrace.Instance);

                // Assert
                Assert.False(result.Success);
                Assert.Null(result.Page);
            }
        }

        private class ConstructableEdgePage : IRazorPage
        {
            public Task Run(Gate.Request req, Gate.Response resp)
            {
                throw new NotImplementedException();
            }
        }

        private class NonConstructableEdgePage : IRazorPage
        {
            private NonConstructableEdgePage()
            {
            }

            public Task Run(Gate.Request req, Gate.Response resp)
            {
                throw new NotImplementedException();
            }
        }

        private class NoParameterlessConstructorEdgePage : IRazorPage
        {
            public NoParameterlessConstructorEdgePage(string foo)
            {
            }

            public Task Run(Gate.Request req, Gate.Response resp)
            {
                throw new NotImplementedException();
            }
        }
    }
}
