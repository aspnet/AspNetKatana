// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
