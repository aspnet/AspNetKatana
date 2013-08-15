// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Hosting.Services;

namespace Microsoft.Owin.Hosting.Starter
{
    /// <summary>
    /// Instantiates instances of the IHostingStarter.
    /// </summary>
    public class HostingStarterActivator : IHostingStarterActivator
    {
        private readonly IServiceProvider _services;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public HostingStarterActivator(IServiceProvider services)
        {
            _services = services;
        }

        /// <summary>
        /// Instantiates instances of the IHostingStarter.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual IHostingStarter Activate(Type type)
        {
            object starter = ActivatorUtilities.GetServiceOrCreateInstance(_services, type);
            return (IHostingStarter)starter;
        }
    }
}
