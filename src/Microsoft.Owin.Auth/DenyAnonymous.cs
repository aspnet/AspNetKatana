﻿// <copyright file="DenyAnonymous.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Microsoft.Owin.Auth
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    // This middleware can be placed at the end of a chain of pass-through auth schemes if at least one type of auth is required.
    public class DenyAnonymous
    {
        private readonly AppFunc _nextApp;

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public DenyAnonymous(AppFunc nextApp)
        {
            _nextApp = nextApp;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            if (environment.Get<IPrincipal>(Constants.ServerUserKey) == null)
            {
                environment[Constants.ResponseStatusCodeKey] = 401;

                return GetCompletedTask();
            }

            return _nextApp(environment);
        }

        private static Task GetCompletedTask()
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.TrySetResult(null);
            return tcs.Task;
        }
    }
}