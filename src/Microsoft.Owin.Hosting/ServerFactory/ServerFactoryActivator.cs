// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Hosting.Services;

namespace Microsoft.Owin.Hosting.ServerFactory
{
    /// <summary>
    /// Used to instantiate the server factory.
    /// </summary>
    public class ServerFactoryActivator : IServerFactoryActivator
    {
        private readonly IServiceProvider _services;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public ServerFactoryActivator(IServiceProvider services)
        {
            _services = services;
        }

        /// <summary>
        /// Instantiate an instance of the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual object Activate(Type type)
        {
            return ActivatorUtilities.GetServiceOrCreateInstance(_services, type);
        }
    }
}
