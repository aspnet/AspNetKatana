// -----------------------------------------------------------------------
// <copyright file="IDirectoryFormatProvider.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Owin.StaticFiles.DirectoryFormatters
{
    /// <summary>
    /// Used to determine which output formatter should be used for a given request
    /// </summary>
    public interface IDirectoryFormatSelector
    {
        /// <summary>
        /// Look up a directory view formatter given the request
        /// </summary>
        /// <param name="environment">The request environment</param>
        /// <param name="formatter">The determined formatter, if any</param>
        /// <returns>True if a formatter was determined</returns>
        bool TryDetermineFormatter(IDictionary<string, object> environment, out IDirectoryInfoFormatter formatter);
    }
}
