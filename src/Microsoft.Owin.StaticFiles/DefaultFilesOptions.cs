// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin.StaticFiles.Infrastructure;

namespace Microsoft.Owin.StaticFiles
{
    /// <summary>
    /// Options for selecting default file names.
    /// </summary>
    public class DefaultFilesOptions : SharedOptionsBase<DefaultFilesOptions>
    {
        /// <summary>
        /// Configuration for the DefaultFilesMiddleware.
        /// </summary>
        public DefaultFilesOptions()
            : this(new SharedOptions())
        {
        }

        /// <summary>
        /// Configuration for the DefaultFilesMiddleware.
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
        /// An ordered list of file names to select by default. List length and ordering may affect performance.
        /// </summary>
        public IList<string> DefaultFileNames { get; private set; }

        /// <summary>
        /// Specifies the file names to select by default, in priority order.
        /// </summary>
        /// <param name="defaultFileNames"></param>
        /// <returns>this</returns>
        public DefaultFilesOptions WithDefaultFileNames(params string[] defaultFileNames)
        {
            DefaultFileNames = defaultFileNames.ToList();
            return this;
        }
    }
}
