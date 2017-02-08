// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Hosting.Starter
{
    /// <summary>
    /// Performs any necessary environment setup prior to executing the IHostingEngine.
    /// </summary>
    public interface IHostingStarter
    {
        /// <summary>
        /// Performs any necessary environment setup prior to executing the IHostingEngine.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        IDisposable Start(StartOptions options);
    }
}
