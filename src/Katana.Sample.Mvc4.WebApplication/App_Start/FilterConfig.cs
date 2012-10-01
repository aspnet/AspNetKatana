using System.Web;
using System.Web.Mvc;

namespace Katana.Sample.Mvc4.WebApplication
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}