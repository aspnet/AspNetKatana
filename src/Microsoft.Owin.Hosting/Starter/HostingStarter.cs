// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Hosting.Starter
{
    /// <summary>
    /// Determines the which IHostingStarter instance to use via the IHostingSterterFactory.
    /// </summary>
    public class HostingStarter : IHostingStarter
    {
        private readonly IHostingStarterFactory _hostingStarterFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostingStarterFactory"></param>
        public HostingStarter(
            IHostingStarterFactory hostingStarterFactory)
        {
            _hostingStarterFactory = hostingStarterFactory;
        }

        /// <summary>
        /// Determines the which IHostingStarter instance to use via the IHostingSterterFactory.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual IDisposable Start(StartOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            string boot;
            options.Settings.TryGetValue("boot", out boot);

            IHostingStarter hostingStarter = _hostingStarterFactory.Create(boot);

            return hostingStarter.Start(options);
        }
    }
}
