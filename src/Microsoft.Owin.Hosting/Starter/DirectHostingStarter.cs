// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Hosting.Engine;

namespace Microsoft.Owin.Hosting.Starter
{
    /// <summary>
    /// Executes the IHostingEngine without making any changes to the current execution environment.
    /// </summary>
    public class DirectHostingStarter : IHostingStarter
    {
        private readonly IHostingEngine _engine;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="engine"></param>
        public DirectHostingStarter(IHostingEngine engine)
        {
            _engine = engine;
        }

        /// <summary>
        /// Executes the IHostingEngine without making any changes to the current execution environment.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual IDisposable Start(StartOptions options)
        {
            return _engine.Start(new StartContext(options));
        }
    }
}
