// <copyright file="ImpersonationMiddleware.cs" company="Katana contributors">
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Microsoft.Owin.Auth
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Impersonates the authenticated user if any.
    /// </summary>
    public class ImpersonationMiddleware
    {
        private readonly AppFunc _next;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public ImpersonationMiddleware(AppFunc next)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }

            _next = next;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public Task Invoke(IDictionary<string, object> environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            // TODO: Under what conditions should we pass through or fail if impersonation is not available?
            object obj;
            IPrincipal user;
            WindowsIdentity identity;
            if (environment.TryGetValue(Constants.ServerUserKey, out obj)
                && (user = obj as IPrincipal) != null
                && (identity = user.Identity as WindowsIdentity) != null
                && (identity.ImpersonationLevel == TokenImpersonationLevel.Impersonation
                    || identity.ImpersonationLevel == TokenImpersonationLevel.Delegation))
            {
                WindowsImpersonationContext context = identity.Impersonate();
                try
                {
                    return _next(environment).ContinueWith(task =>
                    {
                        context.Undo();
                        if (task.IsFaulted)
                        {
                            throw new AggregateException(task.Exception);
                        }
                        if (task.IsCanceled)
                        {
                            throw new TaskCanceledException();
                        }
                    }, TaskContinuationOptions.ExecuteSynchronously);
                }
                catch (Exception)
                {
                    context.Undo();
                    throw;
                }
            }

            return _next(environment);
        }
    }
}
