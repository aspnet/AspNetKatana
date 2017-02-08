// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Owin.Builder
{
    /// <summary>
    /// Simple object used by AppBuilder as seed OWIN callable if the
    /// builder.Properties["builder.DefaultApp"] is not set
    /// </summary>
    internal class NotFound
    {
        private static readonly Task Completed = CreateCompletedTask();

        private static Task CreateCompletedTask()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            return tcs.Task;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            env["owin.ResponseStatusCode"] = 404;
            return Completed;
        }
    }
}
