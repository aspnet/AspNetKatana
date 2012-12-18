using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Owin.Hosting.Services
{
    public class HostingServices : IServiceProvider
    {
        readonly IDictionary<Type, Func<object>> _services = new Dictionary<Type, Func<object>>();
        readonly IDictionary<Type, List<Func<object>>> _priorServices = new Dictionary<Type, List<Func<object>>>();

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
            if (collectionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
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
                        foreach(var factory in prior)
                        {
                            services.Add(factory());
                        }
                    }
                }
                return services;
            }
            return null;
        }

        public HostingServices RemoveAll<T>()
        {
            _services.Remove(typeof(T));
            _priorServices.Remove(typeof(T));
            return this;
        }

        public HostingServices Add<T>(Func<IServiceProvider, T> serviceFactory)
        {
            return Add(() => serviceFactory(this));
        }

        public HostingServices Add<T>(Func<T> serviceFactory)
        {
            Func<object> existing;
            if (_services.TryGetValue(typeof(T), out existing))
            {
                List<Func<object>> prior;
                if (_priorServices.TryGetValue(typeof(T), out prior))
                {
                    prior.Add(existing);
                }
                else
                {
                    prior = new List<Func<object>> { existing };
                    _priorServices.Add(typeof(T), prior);
                }
            }
            _services[typeof(T)] = () => serviceFactory();
            return this;
        }
    }
}
