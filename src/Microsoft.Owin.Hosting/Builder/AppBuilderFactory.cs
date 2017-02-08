// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Owin.Builder;
using Owin;

namespace Microsoft.Owin.Hosting.Builder
{
    /// <summary>
    /// Provides an IAppBuilder instance based on Microsoft.Owin.Builder.AppBuilder.
    /// </summary>
    public class AppBuilderFactory : IAppBuilderFactory
    {
        /// <summary>
        /// Create a new IAppBuilder instance based on Microsoft.Owin.Builder.AppBuilder.
        /// </summary>
        /// <returns></returns>
        public virtual IAppBuilder Create()
        {
            return new AppBuilder();
        }
    }
}
