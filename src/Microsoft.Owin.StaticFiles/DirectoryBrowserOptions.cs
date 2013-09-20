// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
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
            Formatters = new List<IDirectoryInfoFormatter>()
            {
                new HtmlDirectoryFormatter(),
                new JsonDirectoryFormatter(),
                new PlainTextDirectoryFormatter(),
            };
            FormatSelector = new AcceptHeaderDirectoryFormatSelector();
        }

        /// <summary>
        /// The component that examines a request and selects a directory view formatter.
        /// </summary>
        public IDirectoryFormatSelector FormatSelector { get; set; }

        /// <summary>
        /// A list of formatters to select from.
        /// </summary>
        public IList<IDirectoryInfoFormatter> Formatters { get; private set; }

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
