using System;
using System.Collections.Generic;
using System.Text;
using Owin;

namespace Gate
{
    internal static class NotFound
    {
        static readonly ArraySegment<byte> Body = new ArraySegment<byte>(Encoding.UTF8.GetBytes(@"
<!DOCTYPE HTML PUBLIC ""-//IETF//DTD HTML 2.0//EN"">
<html><head>
<title>404 Not Found</title>
</head><body>
<h1>Not Found</h1>
<p>The requested URL was not found on this server.</p>
</body></html>
"));

        public static AppDelegate App()
        {
            return Call;
        }

        public static void Call(IDictionary<string, object> env, ResultDelegate result, Action<Exception> fault)
        {
            result(
                "404 Not Found",
                new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    {"Content-Type", new[] {"text/html"}}
                },
                (write, flush, end, cancellationToken) =>
                {
                    write(Body);
                    end(null);
                });
        }
    }
}
