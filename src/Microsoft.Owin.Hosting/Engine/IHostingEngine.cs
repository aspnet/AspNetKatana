// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Hosting.Engine
{
    /// <summary>
    /// Initializes and starts a web application.
    /// </summary>
    public interface IHostingEngine
    {
        /// <summary>
        /// Initializes and starts a web application.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IDisposable Start(StartContext context);
    }
}
