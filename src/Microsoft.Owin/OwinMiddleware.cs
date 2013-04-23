using System.Threading.Tasks;

namespace Microsoft.Owin
{
    public abstract class OwinMiddleware
    {
        public OwinMiddleware(OwinMiddleware next)
        {
            Next = next;
        }

        public OwinMiddleware Next { get; set; }

        public virtual Task Invoke(OwinRequest request, OwinResponse response)
        {
            return Next.Invoke(request, response);
        }
    }
}
