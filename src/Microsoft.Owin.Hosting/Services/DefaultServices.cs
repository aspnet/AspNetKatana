using System;
using Microsoft.Owin.Hosting.Builder;
using Microsoft.Owin.Hosting.Loader;
using Microsoft.Owin.Hosting.Settings;
using Microsoft.Owin.Hosting.Starter;
using Microsoft.Owin.Hosting.Tracing;

namespace Microsoft.Owin.Hosting.Services
{
    public static class DefaultServices
    {
        public static IServiceProvider Create()
        {
            return Create(_ => { });
        }

        public static IServiceProvider Create(Action<HostingServiceProvider> configuration)
        {
            var services = new HostingServiceProvider();
            ForEach((service, implementation) => services.Add(service, implementation));
            configuration(services);
            return services;
        }

        public static void ForEach(Action<Type, Type> adder)
        {
            ForEach(new SimpleAdder(adder));
        }

        public static void ForEach(IServiceAdder adder)
        {
            adder.Add<IKatanaStarter, KatanaStarter>();
            adder.Add<IHostingStarterFactory, DefaultHostingStarterFactory>();
            adder.Add<IHostingStarterActivator, DefaultHostingStarterActivator>();
            adder.Add<IKatanaEngine, KatanaEngine>();
            adder.Add<IKatanaSettingsProvider, DefaultKatanaSettingsProvider>();
            adder.Add<ITraceOutputBinder, DefaultTraceOutputBinder>();
            adder.Add<IAppLoaderManager, DefaultAppLoaderManager>();
            adder.Add<IAppLoaderProvider, DefaultAppLoaderProvider>();
            adder.Add<IAppActivator, DefaultAppActivator>();
            adder.Add<IAppBuilderFactory, DefaultAppBuilderFactory>();
        }

        public interface IServiceAdder
        {
            void Add<TService, TClass>() where TClass : TService;
        }

        class SimpleAdder : IServiceAdder
        {
            private readonly Action<Type, Type> _adder;

            public SimpleAdder(Action<Type, Type> adder)
            {
                _adder = adder;
            }

            public void Add<TService, TClass>() where TClass : TService
            {
                _adder.Invoke(typeof(TService), typeof(TClass));
            }
        }
    }
}