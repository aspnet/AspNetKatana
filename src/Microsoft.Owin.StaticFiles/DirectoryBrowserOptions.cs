// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.StaticFiles.DirectoryFormatters;
using Microsoft.Owin.StaticFiles.Infrastructure;

namespace Microsoft.Owin.StaticFiles
{
    /// <summary>
    /// Directory browsing options
    /// </summary>
    public class DirectoryBrowserOptions : SharedOptionsBase<DirectoryBrowserOptions>
    {
        /// <summary>
        /// Enabled directory browsing in the current physical directory for all request paths
        /// </summary>
        public DirectoryBrowserOptions()
            : this(new SharedOptions())
        {
        }

        /// <summary>
        /// Enabled directory browsing in the current physical directory for all request paths
        /// </summary>
        /// <param name="sharedOptions"></param>
        public DirectoryBrowserOptions(SharedOptions sharedOptions)
            : base(sharedOptions)
        {
            // Prioritized list
            Formatter = new HtmlDirectoryFormatter();
        }

        /// <summary>
        /// The component that generates the view.
        /// </summary>
        public IDirectoryInfoFormatter Formatter { get; set; }

        /// <summary>
        /// Specifies the component that generates the view.
        /// </summary>
        /// <param name="formatter"></param>
        /// <returns></returns>
        public DirectoryBrowserOptions WithFormatter(IDirectoryInfoFormatter formatter)
        {
            Formatter = formatter;
            return this;
        }
    }
}
