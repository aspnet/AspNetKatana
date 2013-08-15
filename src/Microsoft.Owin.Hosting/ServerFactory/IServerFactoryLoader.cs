// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
