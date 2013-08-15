// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Host.SystemWeb
{
    internal static class Utils
    {
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
