using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gate.Builder.Loader;
using Owin;
using System.Linq;

namespace Gate.Builder
{
#pragma warning disable 811
    using AppFunc = Func< // Call
        IDictionary<string, object>, // Environment
        IDictionary<string, string[]>, // Headers
        Stream, // Body
        CancellationToken, // CallCancelled
        Task<Tuple< //Result
            IDictionary<string, object>, // Properties
            int, // Status
            IDictionary<string, string[]>, // Headers
            Func< // CopyTo
                Stream, // Body
                CancellationToken, // CopyToCancelled
                Task>>>>; // Done

    internal class AppBuilder : IAppBuilder
    {
        public static TApp BuildPipeline<TApp>()
        {
            return BuildPipeline<TApp>(default(string));
        }

        public static TApp BuildPipeline<TApp>(string startupName)
        {
            var startup = new StartupLoader().Load(startupName);
            return BuildPipeline<TApp>(startup);
        }

        public static TApp BuildPipeline<TApp>(Action<IAppBuilder> startup)
        {
            if (startup == null)
                throw new ArgumentNullException("startup");

            var builder = New();
            startup(builder);
            return builder.Materialize<TApp>();
        }

        public AppBuilder() 
        {
            _stack = new List<Delegate>();
            _fallthrough = NotFound.App();
            _properties = new Dictionary<string, object>();
            _adapters = new Dictionary<Tuple<Type, Type>, Func<object, object>>();
            AddStandardAdapters(this);
        }

        AppBuilder(object fallthrough, IDictionary<string, object> properties, IDictionary<Tuple<Type, Type>, Func<object, object>> adapters)
        {
            _stack = new List<Delegate>();
            _fallthrough = fallthrough;
            _properties = properties;
            _adapters = adapters;
        }

        readonly object _fallthrough;
        readonly IList<Delegate> _stack;
        readonly IDictionary<string, object> _properties;
        readonly IDictionary<Tuple<Type, Type>, Func<object, object>> _adapters;

        public static AppBuilder New()
        {
            return new AppBuilder();
        }

        public static AppBuilder New<TApp>(TApp fallthrough)
        {
            var builder = new AppBuilder(fallthrough, new Dictionary<string, object>(), new Dictionary<Tuple<Type, Type>, Func<object, object>>());

            AddStandardAdapters(builder);

            return builder;
        }

        static void AddStandardAdapters(AppBuilder builder)
        {
            builder.AddAdapters<AppDelegate, AppFunc>(Adapters.ToFunc, Adapters.ToDelegate);
        }

        public IDictionary<string, object> Properties
        {
            get { return _properties; }
        }

        public IAppBuilder Use<TApp>(Func<TApp, TApp> middleware)
        {
            _stack.Add(middleware);
            return this;
        }

        public TApp Build<TApp>(Action<IAppBuilder> pipeline)
        {
            var b = new AppBuilder(_fallthrough, _properties, _adapters);
            pipeline(b);
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

        public object Materialize(params Type[] appTypes)
        {
            var app = (object)NotFound.App();
            app = _stack
                .Reverse()
                .Aggregate(app, Wrap);
            return Adapt(app, appTypes);
        }

        object Wrap(object app, Delegate middleware)
        {
            var middlewareFlavor = middleware.Method.ReturnType;
            var neededApp = Adapt(app, middlewareFlavor);
            return middleware.DynamicInvoke(neededApp);
        }

        object Adapt(object currentApp, params Type[] neededFlavors)
        {
            var currentFlavor = currentApp.GetType();
            foreach (var neededFlavor in neededFlavors)
            {
                if (neededFlavor.IsAssignableFrom(currentFlavor))
                    return currentApp;
            }

            foreach (var neededFlavor in neededFlavors)
            {
                foreach (var kv in _adapters)
                {
                    if (neededFlavor.IsAssignableFrom(kv.Key.Item2) &&
                        kv.Key.Item1.IsAssignableFrom(currentFlavor))
                    {
                        return kv.Value.Invoke(currentApp);
                    }
                }
            }

            // todo: find adapter-pair that can satisfy current->needed

            // todo: emit adapter for compatible delegates

            throw new Exception(string.Format("Unable to convert from {0} to {1}", currentFlavor, string.Join(", ", neededFlavors.Select(x => x.ToString()).ToArray())));
        }

        public IAppBuilder AddAdapters<TApp1, TApp2>(Func<TApp1, TApp2> adapter1, Func<TApp2, TApp1> adapter2)
        {
            AddAdapter(adapter1);
            AddAdapter(adapter2);
            return this;
        }

        void AddAdapter<TCurrent, TNeeded>(Func<TCurrent, TNeeded> adapter)
        {
            var key = Tuple.Create(typeof(TCurrent), typeof(TNeeded));
            if (!_adapters.ContainsKey(key))
                _adapters.Add(key, app => adapter((TCurrent)app));
        }
    }
}