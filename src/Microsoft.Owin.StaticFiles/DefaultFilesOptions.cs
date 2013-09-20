// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin.StaticFiles.Infrastructure;

namespace Microsoft.Owin.StaticFiles
{
    /// <summary>
    /// Options for serving default file names.
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
        /// A list of file names to serve by default
        /// </summary>
        public IList<string> DefaultFileNames { get; private set; }

        /// <summary>
        /// Specifies the file names to serve by default
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
