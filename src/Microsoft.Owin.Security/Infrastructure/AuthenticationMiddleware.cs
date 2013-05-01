// <copyright file="AuthenticationMiddleware.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Infrastructure
{
    public abstract class AuthenticationMiddleware<TOptions> : OwinMiddleware where TOptions : AuthenticationOptions
    {
        public AuthenticationMiddleware(OwinMiddleware next, TOptions options)
            : base(next)
        {
            Options = options;
        }

        public TOptions Options { get; set; }

        public override async Task Invoke(OwinRequest request, OwinResponse response)
        {
            AuthenticationHandler<TOptions> handler = CreateHandler();
            await handler.Initialize(Options, request, response);
            if (!await handler.Invoke())
            {
                await Next.Invoke(request, response);
            }
            await handler.Teardown();
        }

        protected abstract AuthenticationHandler<TOptions> CreateHandler();
    }
}
