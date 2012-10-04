//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.AspNet.Owin
{
    internal static class Utils
    {
        /// <summary>
        /// Converts path value to a normal form.
        /// Null values are treated as string.empty.
        /// A path segment is always accompanied by it's leading slash.
        /// A root path is string.empty 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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
