using System;
using System.Collections.Generic;
using System.Threading;
using Gate.Builder.Loader;
using Owin;
using System.Linq;

namespace Gate.Builder
{
#pragma warning disable 811
    using AppAction = Action< // app
       IDictionary<string, object>, // env
       Action< // result
           string, // status
           IDictionary<string, string[]>, // headers
           Action< // body
               Func< // write
                   ArraySegment<byte>, // data                     
                   Action, // continuation
                   bool>, // buffering
               Action< // end
                   Exception>, // error
               CancellationToken>>, // cancel
       Action<Exception>>; // error

    internal class AppBuilder : IAppBuilder
    {
        public static AppDelegate BuildConfiguration()
        {
            return BuildConfiguration(default(string));
        }

        public static AppDelegate BuildConfiguration(string startupName)
        {
            var startup = new StartupLoader().Load(startupName);
            return BuildConfiguration(startup);
        }

        public static AppDelegate BuildConfiguration(Action<IAppBuilder> startup)
        {
            if (startup == null)
                throw new ArgumentNullException("startup");

            var builder = new AppBuilder();
            startup(builder);
            return builder.Materialize();
        }

        readonly IList<Delegate> _stack;

        public AppBuilder()
        {
            _stack = new List<Delegate>();
            AddAdapter<AppDelegate, AppAction>(Adapters.ToAction);
            AddAdapter<AppAction, AppDelegate>(Adapters.ToDelegate);
            AddAdapter<AppDelegate, AppTaskDelegate>(Adapters.ToTaskDelegate);
            AddAdapter<AppTaskDelegate, AppDelegate>(Adapters.ToDelegate);
            AddAdapter<AppAction, AppTaskDelegate>(app => Adapters.ToTaskDelegate(Adapters.ToDelegate(app)));
            AddAdapter<AppTaskDelegate, AppAction>(app => Adapters.ToAction(Adapters.ToDelegate(app)));
        }

        public IAppBuilder Use<TApp>(Func<TApp, TApp> middleware)
        {
            _stack.Add(middleware);
            return this;
        }

        public TApp Build<TApp>(Action<IAppBuilder> fork)
        {
            var b = new AppBuilder();
            fork(b);
            return b.Materialize<TApp>();
        }

        public TApp Materialize<TApp>()
        {
            var app = (Delegate)NotFound.App();
            app = _stack
                .Reverse()
                .Aggregate(app, Wrap);
            return (TApp)(Object)Adapt(app, typeof(TApp));
        }

        Delegate Wrap(Delegate app, Delegate middleware)
        {
            var middlewareFlavor = middleware.Method.ReturnType;
            var neededApp = Adapt(app, middlewareFlavor);
            return (Delegate)middleware.DynamicInvoke(neededApp);
        }

        Delegate Adapt(Delegate currentApp, Type neededFlavor)
        {
            var currentFlavor = currentApp.GetType();
            if (currentFlavor == neededFlavor)
                return currentApp;

            Func<Delegate, Delegate> adapter;
            if (_adapters.TryGetValue(Tuple.Create(currentFlavor, neededFlavor), out adapter))
                return adapter(currentApp);

            throw new Exception(string.Format("Unable to convert from {0} to {1}", currentFlavor, neededFlavor));
        }

        readonly IDictionary<Tuple<Type, Type>, Func<Delegate, Delegate>> _adapters = new Dictionary<Tuple<Type, Type>, Func<Delegate, Delegate>>();
        void AddAdapter<TCurrent, TNeeded>(Func<TCurrent, TNeeded> adapter)
        {
            _adapters.Add(Tuple.Create(typeof(TCurrent), typeof(TNeeded)), Adapter(adapter));
        }
        Func<Delegate, Delegate> Adapter<TCurrent, TNeeded>(Func<TCurrent, TNeeded> adapter)
        {
            return app => (Delegate)(Object)adapter((TCurrent)(Object)app);
        }

        public AppDelegate Build(Action<IAppBuilder> fork)
        {
            var b = new AppBuilder();
            fork(b);
            return b.Materialize<AppDelegate>();
        }

        public AppDelegate Materialize()
        {
            return Materialize<AppDelegate>();
        }

        public IAppBuilder AddAdapters<TApp1, TApp2>(Func<TApp1, TApp2> adapter1, Func<TApp2, TApp1> adapter2)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> Context
        {
            get { throw new NotImplementedException(); }
        }
    }
}