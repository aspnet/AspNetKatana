// <copyright file="ServiceProvider.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Owin.Hosting.Services
{
    public class ServiceProvider : IServiceProvider
    {
        private readonly IDictionary<Type, Func<object>> _services = new Dictionary<Type, Func<object>>();
        private readonly IDictionary<Type, List<Func<object>>> _priorServices = new Dictionary<Type, List<Func<object>>>();

        public ServiceProvider()
        {
            _services[typeof(IServiceProvider)] = () => this;
        }

        public object GetService(Type serviceType)
        {
            return GetSingleService(serviceType) ?? GetMultiService(serviceType);
        }

        private object GetSingleService(Type serviceType)
        {
            Func<object> serviceFactory;
            return _services.TryGetValue(serviceType, out serviceFactory)
                ? serviceFactory.Invoke()
                : null;
        }

        private object GetMultiService(Type collectionType)
        {
            if (collectionType.IsGenericType &&
                collectionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                Type serviceType = collectionType.GetGenericArguments().Single();
                Type listType = typeof(List<>).MakeGenericType(serviceType);
                var services = (IList)Activator.CreateInstance(listType);

                Func<object> serviceFactory;
                if (_services.TryGetValue(serviceType, out serviceFactory))
                {
                    services.Add(serviceFactory());

                    List<Func<object>> prior;
                    if (_priorServices.TryGetValue(serviceType, out prior))
                    {
                        foreach (var factory in prior)
                        {
                            services.Add(factory());
                        }
                    }
                }
                return services;
            }
            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Provided as part of API design")]
        public ServiceProvider RemoveAll<T>()
        {
            return RemoveAll(typeof(T));
        }

        public ServiceProvider RemoveAll(Type type)
        {
            _services.Remove(type);
            _priorServices.Remove(type);
            return this;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Provided as part of API design")]
        public ServiceProvider AddInstance<TService>(object instance)
        {
            return AddInstance(typeof(TService), instance);
        }

        public ServiceProvider AddInstance(Type service, object instance)
        {
            return Add(service, () => instance);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Provided as part of API design")]
        public ServiceProvider Add<TService, TImplementation>()
        {
            return Add(typeof(TService), typeof(TImplementation));
        }

        public ServiceProvider Add(Type serviceType, Type implementationType)
        {
            Func<IServiceProvider, object> factory = ActivatorUtilities.CreateFactory(implementationType);
            return Add(serviceType, () => factory(this));
        }

        public ServiceProvider Add(Type serviceType, Func<object> serviceFactory)
        {
            Func<object> existing;
            if (_services.TryGetValue(serviceType, out existing))
            {
                List<Func<object>> prior;
                if (_priorServices.TryGetValue(serviceType, out prior))
                {
                    prior.Add(existing);
                }
                else
                {
                    prior = new List<Func<object>> { existing };
                    _priorServices.Add(serviceType, prior);
                }
            }
            _services[serviceType] = serviceFactory;
            return this;
        }
    }
}
