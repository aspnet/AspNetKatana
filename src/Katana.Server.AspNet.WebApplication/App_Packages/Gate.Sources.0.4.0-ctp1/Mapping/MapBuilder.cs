using System;
using System.Collections.Generic;
using Owin;

namespace Gate.Mapping
{
    class MapBuilder : IAppBuilder
    {
        readonly IAppBuilder _builder;
        readonly IDictionary<string, AppDelegate> _map;
        readonly Func<AppDelegate, IDictionary<string, AppDelegate>, AppDelegate> _mapper;

        public MapBuilder(IAppBuilder builder, Func<AppDelegate, IDictionary<string, AppDelegate>, AppDelegate> mapper)
        {
            _map = new Dictionary<string, AppDelegate>();
            _mapper = mapper;
            _builder = builder.Use(a => _mapper(a, _map));
        }

        public void MapInternal(string path, AppDelegate app)
        {
            _map[path] = app;
        }

        public IAppBuilder Use<TApp>(Func<TApp, TApp> middleware)
        {
            return _builder.Use(middleware);
        }

        public TApp Build<TApp>(Action<IAppBuilder> fork)
        {
            return _builder.Build<TApp>(fork);
        }

        public IAppBuilder AddAdapters<TApp1, TApp2>(Func<TApp1, TApp2> adapter1, Func<TApp2, TApp1> adapter2)
        {
            return _builder.AddAdapters(adapter1, adapter2);
        }

        public IDictionary<string, object> Context
        {
            get { return _builder.Context; }
        }
    }
}
