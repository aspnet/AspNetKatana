// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Hosting.Starter
{
    /// <summary>
    /// Instantiates instances of the IHostingStarter.
    /// </summary>
    public interface IHostingStarterActivator
    {
        /// <summary>
        /// Instantiates instances of the IHostingStarter.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IHostingStarter Activate(Type type);
    }
}
