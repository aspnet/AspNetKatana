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
        /// 
        /// </summary>
        /// <param name="sharedOptions"></param>
        public DirectoryBrowserOptions(SharedOptions sharedOptions)
            : base(sharedOptions)
        {
            FormatSelector = new AcceptHeaderDirectoryFormatSelector();
        }

        /// <summary>
        /// The component that examines a request and selects a directory view formatter.
        /// </summary>
        public IDirectoryFormatSelector FormatSelector { get; private set; }

        /// <summary>
        /// Specifies component that examines a request and selects a directory view formatter.
        /// </summary>
        /// <param name="formatSelector"></param>
        /// <returns></returns>
        public DirectoryBrowserOptions WithFormatSelector(IDirectoryFormatSelector formatSelector)
        {
            FormatSelector = formatSelector;
            return this;
        }
    }
}
