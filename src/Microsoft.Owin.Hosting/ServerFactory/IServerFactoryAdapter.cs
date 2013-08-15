// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Owin;

namespace Microsoft.Owin.Hosting.ServerFactory
{
    /// <summary>
    /// The basic ServerFactory contract.
    /// </summary>
    public interface IServerFactoryAdapter
    {
        /// <summary>
        /// An optional method that allows the server factory to specify its capabilities.
        /// </summary>
        /// <param name="builder"></param>
        void Initialize(IAppBuilder builder);

        /// <summary>
        /// Starts a server with the given app instance.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        IDisposable Create(IAppBuilder builder);
    }
}
