// -----------------------------------------------------------------------
// <copyright file="DirectoryBrowserOptions.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Owin.StaticFiles.DirectoryFormatters;
using Microsoft.Owin.StaticFiles.FileSystems;
using Microsoft.Owin.StaticFiles.Infrastructure;

namespace Microsoft.Owin.StaticFiles
{
    public class DirectoryBrowserOptions : SharedOptionsBase<DirectoryBrowserOptions>
    {
        public DirectoryBrowserOptions()
            : this(new SharedOptions())
        {
        }

        public DirectoryBrowserOptions(SharedOptions sharedOptions)
            : base(sharedOptions)
        {
            FormatSelector = new AcceptHeaderDirectoryFormatSelector();
        }

        public IDirectoryFormatSelector FormatSelector { get; set; }

        public DirectoryBrowserOptions WithFormatSelector(IDirectoryFormatSelector formatSelector)
        {
            FormatSelector = formatSelector;
            return this;
        }
    }
}
