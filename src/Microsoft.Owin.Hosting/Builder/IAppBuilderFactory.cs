// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Owin;

namespace Microsoft.Owin.Hosting.Builder
{
    /// <summary>
    /// Provides an IAppBuilder instance.
    /// </summary>
    public interface IAppBuilderFactory
    {
        /// <summary>
        /// Create a new IAppBuilder instance.
        /// </summary>
        /// <returns></returns>
        IAppBuilder Create();
    }
}
