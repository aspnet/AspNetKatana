// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.FileSystems;

namespace Microsoft.Owin.StaticFiles
{
    /// <summary>
    /// Used to apply access policies for the static file middleware.
    /// </summary>
    public interface IFileAccessPolicy
    {
        /// <summary>
        /// Indicates if the given request should have access to the given file.
        /// </summary>
        /// <param name="context"></param>
        void CheckPolicy(FileAccessPolicyContext context);
    }
}
