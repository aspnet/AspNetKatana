using Owin;

namespace Gate.Middleware
{
    internal static class MethodOverride
    {
        public static IAppBuilder UseMethodOverride(this IAppBuilder builder)
        {
            return builder.Use(Middleware);
        }

        public static AppDelegate Middleware(AppDelegate app)
        {
            return call =>
            {
                var req = new Request(call);
                var method = req.Headers.GetHeader("x-http-method-override");
                if (!string.IsNullOrWhiteSpace(method))
                    req.Method = method;

                return app(call);
            };
        }
    }
}
