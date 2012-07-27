using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Owin;

namespace Gate.Middleware
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

        public static Task<ResultParameters> Call(CallParameters call)
        {
            return TaskHelpers.FromResult(new ResultParameters
            {
                Status = 404,
                Headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
                {
                    {"Content-Type", new[] {"text/html"}}
                },
                Body = (output, _) =>
                {
                    output.Write(Body.Array, Body.Offset, Body.Count);
                    return TaskHelpers.Completed();
                },
                Properties = new Dictionary<string, object>()
            });
        }
    }
}
