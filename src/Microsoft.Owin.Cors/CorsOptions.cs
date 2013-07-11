using System.Web.Cors;

namespace Microsoft.Owin.Cors
{
    public class CorsOptions
    {
        public CorsPolicy CorsPolicy { get; set; }
        public ICorsEngine CorsEngine { get; set; }
    }
}
