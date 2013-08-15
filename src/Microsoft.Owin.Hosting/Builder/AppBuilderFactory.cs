// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
