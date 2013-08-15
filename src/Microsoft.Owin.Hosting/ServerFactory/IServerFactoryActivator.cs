// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Hosting.ServerFactory
{
    /// <summary>
    /// Used to instantiate the server factory.
    /// </summary>
    public interface IServerFactoryActivator
    {
        /// <summary>
        /// Instantiate an instance of the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        object Activate(Type type);
    }
}
