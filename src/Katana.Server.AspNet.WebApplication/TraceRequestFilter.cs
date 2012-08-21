using System.Diagnostics;
using System.Net.Http;

namespace Microsoft.AspNet.Owin.WebApplication
{
    public class TraceRequestFilter : DelegatingHandler
    {
        public TraceRequestFilter()
        {
        }

        public TraceRequestFilter(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            Trace.WriteLine(string.Format("RequestUri {0}", request.RequestUri));
            return base.SendAsync(request, cancellationToken);
        }
    }
}