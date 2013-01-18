// -----------------------------------------------------------------------
// <copyright file="DefaultFilesOptions.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin.StaticFiles.Infrastructure;

namespace Microsoft.Owin.StaticFiles
{
    /// <summary>
    /// Options for serving default file names
    /// </summary>
    public class DefaultFilesOptions : SharedOptionsBase<DefaultFilesOptions>
    {
        /// <summary>
        /// 
        /// </summary>
        public DefaultFilesOptions()
            : this(new SharedOptions())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sharedOptions"></param>
        public DefaultFilesOptions(SharedOptions sharedOptions)
            : base(sharedOptions)
        {
            // Prioritized list
            DefaultFileNames = new List<string>()
            {
                "default.htm",
                "default.html",
                "index.htm",
                "index.html",
            };
        }

        /// <summary>
        /// A list of file names to serve by default
        /// </summary>
        public IList<string> DefaultFileNames { get; private set; }

        /// <summary>
        /// Specifies the file names to serve by default
        /// </summary>
        /// <param name="defaultFileNames"></param>
        /// <returns>this</returns>
        public DefaultFilesOptions WithDefaultFileNames(IEnumerable<string> defaultFileNames)
        {
            DefaultFileNames = defaultFileNames.ToList();
            return this;
        }
    }
}
