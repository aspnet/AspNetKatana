// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
        bool TryDetermineFormatter(IOwinContext context, out IDirectoryInfoFormatter formatter);
    }
}
