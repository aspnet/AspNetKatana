using System;
using Microsoft.Owin.Hosting.Settings;

namespace Microsoft.Owin.Hosting.Services
{
    public static class DefaultServices
    {
        public static IServiceProvider Create()
        {
            return Create(_ => { });
        }

        public static IServiceProvider Create(Action<HostingServices> configuration)
        {
            var services = new HostingServices()
                .Add(KatanaEngine.CreateInstance)
                .Add(DefaultKatanaSettingsProvider.CreateInstance)
                .Add(DefaultTraceOutputBinder.CreateInstance)
                .Add(DefaultAppLoaderChain.CreateInstance)
                .Add(DefaultAppLoader.CreateInstance)
                .Add(DefaultAppActivator.CreateInstance)
                .Add(DefaultAppBuilderFactory.CreateInstance);

            configuration(services);
            return services;
        }


        public static void ForEach(IServiceAdder adder)
        {
            adder.Add<IKatanaEngine, KatanaEngine>();
            adder.Add<IKatanaSettingsProvider, DefaultKatanaSettingsProvider>();
            adder.Add<ITraceOutputBinder, DefaultTraceOutputBinder>();
            adder.Add<IAppLoaderChain, DefaultAppLoaderChain>();
            adder.Add<IAppLoader, DefaultAppLoader>();
            adder.Add<IAppActivator, DefaultAppActivator>();
            adder.Add<IAppBuilderFactory, DefaultAppBuilderFactory>();
        }
        public static void ForEach(Action<Type, Type> adder)
        {
            ForEach(new SimpleAdder(adder));
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