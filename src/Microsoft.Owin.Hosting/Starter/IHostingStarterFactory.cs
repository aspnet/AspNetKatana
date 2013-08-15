// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Hosting.Starter
{
    /// <summary>
    /// Creates a IHostingStarter for the given identifier.
    /// </summary>
    public interface IHostingStarterFactory
    {
        /// <summary>
        /// Creates a IHostingStarter for the given identifier.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IHostingStarter Create(string name);
    }
}
