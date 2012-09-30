using System.Collections.Generic;
using System.Threading.Tasks;

namespace Owin.Builder
{
    /// <summary>
    /// Simple object used by AppBuilder as seed OWIN callable if the
    /// builder.Properties["builder.DefaultApp"] is not set
    /// </summary>
    class NotFound
    {
        static readonly Task Completed;

        static NotFound()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            Completed = tcs.Task;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            env["owin.ResponseStatusCode"] = 404;
            return Completed;
        }
    }
}
