using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;


namespace Katana.Boot.AspNet
{
    public class AspNetCaller
    {
        readonly Func<IDictionary<string, object>, Task> _next;

        public AspNetCaller(Func<IDictionary<string, object>, Task> next)
        {
            _next = next;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var workerRequest = new KatanaWorkerRequest(env);
            HttpRuntime.ProcessRequest(workerRequest);
            return workerRequest.Completed;
        }
    }
}
