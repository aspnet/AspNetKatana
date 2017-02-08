// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
