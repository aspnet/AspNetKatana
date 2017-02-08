// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Owin.Hosting.ServerFactory
{
    /// <summary>
    /// Used to locate and load the named server factory.
    /// </summary>
    public interface IServerFactoryLoader
    {
        /// <summary>
        /// Used to locate and load the named server factory.
        /// </summary>
        /// <param name="serverName"></param>
        /// <returns></returns>
        IServerFactoryAdapter Load(string serverName);
    }
}
