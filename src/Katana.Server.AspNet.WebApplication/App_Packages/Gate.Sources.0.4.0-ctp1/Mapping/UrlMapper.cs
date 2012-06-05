using System;
using System.Collections.Generic;
using System.Linq;
using Owin;

namespace Gate.Mapping
{
    internal class UrlMapper
    {
        readonly AppDelegate _app;
        IEnumerable<Tuple<string, AppDelegate>> _map = Enumerable.Empty<Tuple<string, AppDelegate>>();

        UrlMapper(AppDelegate app)
        {
            _app = app;
        }

        public static AppDelegate Create(IDictionary<string, AppDelegate> map)
        {
            return Create(NotFound.App(), map);
        }

        public static AppDelegate Create(AppDelegate app, IDictionary<string, AppDelegate> map)
        {
            if (app == null)
                throw new ArgumentNullException("app");

            var mapper = new UrlMapper(app);
            mapper.Remap(map);
            return mapper.Call;
        }

        public void Remap(IDictionary<string, AppDelegate> map)
        {
            _map = map
                .Select(kv => Tuple.Create(kv.Key, kv.Value))
                .OrderByDescending(m => m.Item1.Length)
                .ToArray();
        }

        public void Call(
            IDictionary<string, object> env,
            ResultDelegate result,
            Action<Exception> fault)
        {
            var paths = new Paths(env);
            var path = paths.Path;
            var pathBase = paths.PathBase;
            Action finish = () =>
            {
                paths.Path = path;
                paths.PathBase = pathBase;
            };
            var match = _map.FirstOrDefault(m => path.StartsWith(m.Item1));
            if (match == null)
            {
                // fall-through to default
                _app(env, result, fault);
                return;
            }
            paths.PathBase = pathBase + match.Item1;
            paths.Path = path.Substring(match.Item1.Length);
            match.Item2.Invoke(env, result, fault);
        }

        /// <summary>
        /// This is a very small version of Environment, repeated here so the Gate.Builder.dll
        /// doesn't need to take a hard dependency on Gate.dll
        /// </summary>
        class Paths
        {
            readonly IDictionary<string, object> _env;

            public Paths(IDictionary<string, object> env)
            {
                _env = env;
            }

            const string RequestPathBaseKey = OwinConstants.RequestPathBase;
            const string RequestPathKey = OwinConstants.RequestPath;

            public string Path
            {
                get
                {
                    object value;
                    return _env.TryGetValue(RequestPathKey, out value) ? Convert.ToString(value) : null;
                }
                set
                {
                    _env[RequestPathKey] = value;
                }
            }

            public string PathBase
            {
                get
                {
                    object value;
                    return _env.TryGetValue(RequestPathBaseKey, out value) ? Convert.ToString(value) : null;
                }
                set
                {
                    _env[RequestPathBaseKey] = value;
                }
            }
        }
    }
}
