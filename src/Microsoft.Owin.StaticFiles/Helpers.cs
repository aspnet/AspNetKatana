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
    }
}
