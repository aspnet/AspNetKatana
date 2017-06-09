// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.FileSystems;

namespace Microsoft.Owin.StaticFiles
{
    /// <summary>
    /// Custom provider for ETags.
    /// </summary>
    public interface ICustomEtagProvider
    {
        string CalculateEtagHash(IFileInfo fileInfo);
    }
}