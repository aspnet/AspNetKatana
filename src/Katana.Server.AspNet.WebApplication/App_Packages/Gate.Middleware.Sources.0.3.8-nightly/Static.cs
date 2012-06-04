using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gate.Middleware.StaticFiles;
using Owin;

namespace Gate.Middleware
{
    internal static class StaticExtensions
    {
        public static IAppBuilder UseStatic(this IAppBuilder builder, string root, IEnumerable<string> urls)
        {
            return builder.Use(Static.Middleware, root, urls);
        }

        public static IAppBuilder UseStatic(this IAppBuilder builder, IEnumerable<string> urls)
        {
            return builder.Use(Static.Middleware, urls);
        }

        public static IAppBuilder UseStatic(this IAppBuilder builder, string root)
        {
            return builder.Use(Static.Middleware, root);
        }

        public static IAppBuilder UseStatic(this IAppBuilder builder)
        {
            return builder.Use(Static.Middleware);
        }
    }

    internal class Static
    {
        private readonly AppDelegate app;
        private readonly FileServer fileServer;
        private readonly IEnumerable<string> urls;

        public Static(AppDelegate app, IEnumerable<string> urls)
            : this(app, null, urls)
        { }

        public Static(AppDelegate app, string root = null, IEnumerable<string> urls = null)
        {
            this.app = app;

            if (root == null)
            {
                root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "public");
            }

            if (!Directory.Exists(root))
            {
                throw new DirectoryNotFoundException(string.Format("Invalid root directory: {0}", root));
            }

            if (urls == null)
            {
                var rootDirectory = new DirectoryInfo(root);
                var files = rootDirectory.GetFiles("*").Select(fi => "/" + fi.Name);
                var directories = rootDirectory.GetDirectories().Select(di => "/" + di.Name);
                urls = files.Concat(directories);
            }

            this.urls = urls;

            fileServer = new FileServer(root);
        }

        public static AppDelegate Middleware(AppDelegate app, string root, IEnumerable<string> urls)
        {
            return new Static(app, root, urls).Invoke;
        }

        public static AppDelegate Middleware(AppDelegate app, string root)
        {
            return new Static(app, root).Invoke;
        }

        public static AppDelegate Middleware(AppDelegate app, IEnumerable<string> urls)
        {
            return new Static(app, urls).Invoke;
        }

        public static AppDelegate Middleware(AppDelegate app)
        {
            return new Static(app).Invoke;
        }

        public void Invoke(IDictionary<string, object> env, ResultDelegate result, Action<Exception> fault)
        {
            var path = env[OwinConstants.RequestPath].ToString();

            if (urls.Any(path.StartsWith))
            {
                fileServer.Invoke(env, result, fault);
                return;
            }

            app.Invoke(env, result, fault);
        }
    }
}
