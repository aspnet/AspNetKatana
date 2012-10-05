// Copyright 2011-2012 Katana contributors
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
using Shouldly;
using Xunit;

namespace Microsoft.AspNet.Owin.Tests
{
    public class OwinHttpHandlerTests : TestsBase
    {
        [Fact]
        public void ProcessRequestIsNotImplemeted()
        {
            var httpHandler = new OwinHttpHandler(string.Empty, () => null);
            var httpContext = NewHttpContext(new Uri("http://localhost"));

            Should.Throw<NotImplementedException>(() => httpHandler.ProcessRequest(httpContext));
        }

        [Fact]
        public Task ItShouldCallAppDelegateWhenBeginProcessRequestCalled()
        {
            var httpHandler = new OwinHttpHandler(string.Empty, WasCalledApp);
            var httpContext = NewHttpContext(new Uri("http://localhost"));

            var task = Task.Factory.FromAsync(httpHandler.BeginProcessRequest, httpHandler.EndProcessRequest, httpContext, null);
            return task.ContinueWith(_ =>
            {
                task.Exception.ShouldBe(null);
                WasCalled.ShouldBe(true);
            });
        }
    }
}
