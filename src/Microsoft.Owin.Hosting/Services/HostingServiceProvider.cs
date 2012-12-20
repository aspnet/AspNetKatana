using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Microsoft.Owin.Hosting.Services
{
    public class HostingServiceProvider : IServiceProvider
    {
        readonly IDictionary<Type, Func<object>> _services = new Dictionary<Type, Func<object>>();
        readonly IDictionary<Type, List<Func<object>>> _priorServices = new Dictionary<Type, List<Func<object>>>();

        public HostingServiceProvider()
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
                var serviceType = collectionType.GetGenericArguments().Single();
                var listType = typeof(List<>).MakeGenericType(serviceType);
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

        public HostingServiceProvider RemoveAll<T>()
        {
            _services.Remove(typeof(T));
            _priorServices.Remove(typeof(T));
            return this;
        }

        public HostingServiceProvider AddInstance<TService>(object instance)
        {
            return Add(typeof(TService), () => instance);
        }

        public HostingServiceProvider Add<TService, TImplementation>()
        {
            return Add(typeof(TService), typeof(TImplementation));
        }

        public HostingServiceProvider Add(Type serviceType, Type implementationType)
        {
            var factory = ActivatorUtils.CreateFactory(implementationType);
            return Add(serviceType, () => factory(this));
        }

        public HostingServiceProvider Add(Type serviceType, Func<object> serviceFactory)
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
