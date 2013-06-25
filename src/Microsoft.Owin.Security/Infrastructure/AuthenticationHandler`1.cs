// <copyright file="AuthenticationHandler.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Infrastructure
{
    /// <summary>
    /// Base class for the per-request work performed by most authentication middleware.
    /// </summary>
    /// <typeparam name="TOptions">Specifies which type for of AuthenticationOptions property</typeparam>
    public abstract class AuthenticationHandler<TOptions> : AuthenticationHandler where TOptions : AuthenticationOptions
    {
        protected TOptions Options { get; private set; }

        /// <summary>
        /// Initialize is called once per request to contextualize this instance with appropriate state.
        /// </summary>
        /// <param name="options">The original options passed by the application control behavior</param>
        /// <param name="context">The utility object to observe the current request and response</param>
        /// <returns>async completion</returns>
        internal Task Initialize(TOptions options, IOwinContext context)
        {
            Options = options;
            return BaseInitialize(options, context);
        }
    }
}
