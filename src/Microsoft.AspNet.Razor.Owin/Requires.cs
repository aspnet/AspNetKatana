// -----------------------------------------------------------------------
// <copyright file="Requires.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Microsoft.AspNet.Razor.Owin
{
    internal static class Requires
    {
        public static void NotNullOrEmpty(string value, string paramName)
        {
            if (String.IsNullOrEmpty(value))
            { 
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture, 
                        Resources.Argument_NotNullOrEmpty, 
                        paramName),
                    paramName); 
            }
        }

        public static void NotNull(object value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void InRange(bool condition, string paramName)
        {
            if (!condition)
            {
                throw new ArgumentOutOfRangeException(paramName);
            }
        }
    }
}