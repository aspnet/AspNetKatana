// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Hosting.Services
{
    /// <summary>
    /// Extension methods for IServiceProvider.
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Retrieve a service of type T from the IServiceProvider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static T GetService<T>(this IServiceProvider services)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }

            return (T)services.GetService(typeof(T));
        }
    }
}
