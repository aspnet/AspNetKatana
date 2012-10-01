using System;
using Owin;
using System.Threading.Tasks;
using System.Collections.Generic;
using Gate;
using System.IO;

namespace Katana.Sample.HelloMono
{
	public class Startup
	{
		public void Configuration (IAppBuilder builder)
		{
			var output = (TextWriter)builder.Properties["host.TraceOutput"];
			output.WriteLine("Starting");

			builder.UseFunc(Hello);
		}

		public Func<IDictionary<string,object>,Task> Hello(Func<IDictionary<string,object>,Task> next)
		{
			return env =>
			{
				var req = new Request(env);

				req.TraceOutput.WriteLine("Request {0} at {1}{2}", req.Method,req.PathBase,req.Path);

				if (!req.Path.StartsWith("/hello", StringComparison.OrdinalIgnoreCase))
				{
					return next(env);
				}

				var resp = new Response(env);
				resp.ContentType="text/plain";
				return resp.WriteAsync("Hello, mono");
			};
		}
	}
}

