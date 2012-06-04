using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Katana.Server.AspNet
{
    static class Utils
    {
        /// <summary>
        /// Converts path value to a normal form.
        /// Null values are treated as string.empty.
        /// A path segment is always accompanied by it's leading slash.
        /// A root path is string.empty 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string NormalizePath(string path)
        {
            return string.IsNullOrEmpty(path) ? (path ?? string.Empty) : (path[0] == '/' ? path : '/' + path);
        }
    }
}
