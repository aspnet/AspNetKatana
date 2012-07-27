namespace Gate.Middleware
{
    using System.IO;
    using System.Threading.Tasks;
    using Gate.Utils;
    using Owin;

    internal static class ContentLength
    {
        public static IAppBuilder UseContentLength(this IAppBuilder builder)
        {
            return builder.Use<AppDelegate>(Middleware);
        }
        
        public static AppDelegate Middleware(AppDelegate app)
        {
            return call =>
            {
                return app(call).Then<ResultParameters, ResultParameters>( 
                    result =>
                    {
                        if (IsStatusWithNoNoEntityBody(result.Status)
                            || result.Headers.ContainsKey("Content-Length") 
                            || result.Headers.ContainsKey("Transfer-Encoding"))
                        {
                            return TaskHelpers.FromResult(result);
                        }

                        if (result.Body == null)
                        {
                            result.Headers.SetHeader("Content-Length", "0");
                            return TaskHelpers.FromResult(result);
                        }

                        // Buffer the body
                        MemoryStream buffer = new MemoryStream();
                        return result.Body(buffer, call.Completed).Then<ResultParameters>(
                            () =>
                            {
                                buffer.Seek(0, SeekOrigin.Begin);
                                result.Headers.SetHeader("Content-Length", buffer.Length.ToString());
                                result.Body = (output, cancel) =>
                                {
                                    return buffer.CopyToAsync(output, cancel);
                                };

                                return TaskHelpers.FromResult(result);
                            }, call.Completed);

                    }, call.Completed);
            };
        }

        private static bool IsStatusWithNoNoEntityBody(int status)
        {
            return (status >= 100 && status < 200) ||
                status == 204 ||
                status == 205 ||
                status == 304;
        }
    }
}

