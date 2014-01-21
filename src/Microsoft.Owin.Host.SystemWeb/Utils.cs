// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal static class Utils
    {
        internal static Task CompletedTask = CreateCompletedTask();
        internal static Task CancelledTask = CreateCancelledTask();

        private static Task CreateCompletedTask()
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.TrySetResult(null);
            return tcs.Task;
        }

        private static Task CreateCancelledTask()
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.TrySetCanceled();
            return tcs.Task;
        }

        internal static Task CreateFaultedTask(Exception ex)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.TrySetException(ex);
            return tcs.Task;
        }

        // Converts path value to a normal form.
        // Null values are treated as string.empty.
        // A path segment is always accompanied by it's leading slash.
        // A root path is string.empty
        internal static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path ?? string.Empty;
            }
            if (path.Length == 1)
            {
                return path[0] == '/' ? string.Empty : '/' + path;
            }
            return path[0] == '/' ? path : '/' + path;
        }
    }
}
