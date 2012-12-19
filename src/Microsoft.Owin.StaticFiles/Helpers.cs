// -----------------------------------------------------------------------
// <copyright file="Helpers.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Microsoft.Owin.StaticFiles
{
    internal static class Helpers
    {
        internal static bool IsGetOrHeadMethod(IDictionary<string, object> environment)
        {
            string method = (string)environment[Constants.RequestMethod];
            return "GET".Equals(method, StringComparison.OrdinalIgnoreCase)
                || "HEAD".Equals(method, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsGetMethod(IDictionary<string, object> environment)
        {
            string method = (string)environment[Constants.RequestMethod];
            return "GET".Equals(method, StringComparison.OrdinalIgnoreCase);
        }

        internal static CancellationToken GetCancellationToken(IDictionary<string, object> environment)
        {
            return (CancellationToken)environment[Constants.CallCancelledKey];
        }

        internal static bool PathEndsInSlash(IDictionary<string, object> environment)
        {
            string path = (string)environment[Constants.RequestPathKey];
            return path.EndsWith("/", StringComparison.Ordinal);
        }

        internal static bool TryMatchPath(IDictionary<string, object> environment, string matchUrl, bool forDirectory, out string subpath)
        {
            string path = (string)environment[Constants.RequestPathKey];

            if (forDirectory && (path.Length == 0 || path[path.Length - 1] != '/'))
            {
                path += "/";
            }

            if (path.StartsWith(matchUrl, StringComparison.OrdinalIgnoreCase)
                && (path.Length == matchUrl.Length 
                    || path[matchUrl.Length] == '/'
                    || (matchUrl.Length > 0 && matchUrl[matchUrl.Length - 1] == '/')))
            {
                subpath = path.Substring(matchUrl.Length);
                return true;
            }
            subpath = null;
            return false;
        }
    }
}
