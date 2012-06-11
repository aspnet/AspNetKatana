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
        readonly IDictionary<string, object> _context;
        readonly IDictionary<Tuple<Type, Type>, Func<object, object>> _adapters = new Dictionary<Tuple<Type, Type>, Func<object, object>>();

        public AppBuilder()
        {
            _stack = new List<Delegate>();
            _context = new Dictionary<string, object>();
            AddAdapter<AppDelegate, AppAction>(Adapters.ToAction);
            AddAdapter<AppAction, AppDelegate>(Adapters.ToDelegate);
            AddAdapter<AppDelegate, AppTaskDelegate>(Adapters.ToTaskDelegate);
            AddAdapter<AppTaskDelegate, AppDelegate>(Adapters.ToDelegate);
            AddAdapter<AppAction, AppTaskDelegate>(app => Adapters.ToTaskDelegate(Adapters.ToDelegate(app)));
            AddAdapter<AppTaskDelegate, AppAction>(app => Adapters.ToAction(Adapters.ToDelegate(app)));
        }

        public AppBuilder(IDictionary<string, object> context, IDictionary<Tuple<Type, Type>, Func<object, object>> adapters)
        {
            _stack = new List<Delegate>();
            _context = context;
            _adapters = adapters;
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
            var app = (object)NotFound.App();
            app = _stack
                .Reverse()
                .Aggregate(app, Wrap);
            return (TApp)Adapt(app, typeof(TApp));
        }

        object Wrap(object app, Delegate middleware)
        {
            var middlewareFlavor = middleware.Method.ReturnType;
            var neededApp = Adapt(app, middlewareFlavor);
            return middleware.DynamicInvoke(neededApp);
        }

        object Adapt(object currentApp, Type neededFlavor)
        {
            var currentFlavor = currentApp.GetType();
            if (neededFlavor.IsAssignableFrom(currentFlavor))
                return currentApp;

            foreach (var kv in _adapters)
            {
                if (neededFlavor.IsAssignableFrom(kv.Key.Item2) &&
                    kv.Key.Item1.IsAssignableFrom(currentFlavor))
                {
                    return kv.Value.Invoke(currentApp);
                }
            }

            throw new Exception(string.Format("Unable to convert from {0} to {1}", currentFlavor, neededFlavor));
        }


        void AddAdapter<TCurrent, TNeeded>(Func<TCurrent, TNeeded> adapter)
        {
            var key = Tuple.Create(typeof(TCurrent), typeof(TNeeded));
            if (!_adapters.ContainsKey(key))
                _adapters.Add(key, Adapter(adapter));
        }

        Func<object, object> Adapter<TCurrent, TNeeded>(Func<TCurrent, TNeeded> adapter)
        {
            return app => adapter((TCurrent)app);
        }

        public AppDelegate Build(Action<IAppBuilder> fork)
        {
            var b = new AppBuilder(_context, _adapters);
            fork(b);
            return b.Materialize<AppDelegate>();
        }

        public AppDelegate Materialize()
        {
            return Materialize<AppDelegate>();
        }

        public IAppBuilder AddAdapters<TApp1, TApp2>(Func<TApp1, TApp2> adapter1, Func<TApp2, TApp1> adapter2)
        {
            AddAdapter(adapter1);
            AddAdapter(adapter2);
            return this;
        }

        public IDictionary<string, object> Context
        {
            get { return _context; }
        }
    }
}