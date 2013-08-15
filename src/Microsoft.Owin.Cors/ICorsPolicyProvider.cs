using System.Threading.Tasks;
using System.Web.Cors;

namespace Microsoft.Owin.Cors
{
    public interface ICorsPolicyProvider
    {
        Task<CorsPolicy> GetCorsPolicyAsync(IOwinRequest request);
    }
}
