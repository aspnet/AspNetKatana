// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Hosting.Builder
{
    /// <summary>
    /// Used to instantiate the application entry point. e.g. the Startup class.
    /// </summary>
    public interface IAppActivator
    {
        /// <summary>
        /// Instantiate an instance of the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        object Activate(Type type);
    }
}
