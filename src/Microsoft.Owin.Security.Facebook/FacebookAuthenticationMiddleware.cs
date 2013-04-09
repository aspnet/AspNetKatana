// <copyright file="FacebookAuthenticationMiddleware.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Microsoft.Owin.Security.Facebook
{
    public class FacebookAuthenticationMiddleware
    {
        private readonly Func<IDictionary<string, object>, Task> _next;
        private readonly FacebookAuthenticationOptions _options;

        public FacebookAuthenticationMiddleware(
            Func<IDictionary<string, object>, Task> next,
            FacebookAuthenticationOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            var context = new FacebookAuthenticationContext(
                _options,
                env);

            await context.Initialize();
            if (!await context.Invoke())
            {
                await _next(env);
            }
            context.Teardown();
        }
    }
}
