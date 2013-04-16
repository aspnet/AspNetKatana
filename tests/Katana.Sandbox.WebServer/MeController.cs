using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace Katana.Sandbox.WebServer
{
    public class Me
    {
        public List<Detail> Details { get; set; }
    }

    public class Detail
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class MeController : ApiController
    {
        public HttpResponseMessage Get()
        {
            var user = HttpContext.Current.User;
            var identity = user.Identity as ClaimsIdentity ?? new ClaimsIdentity(user.Identity);
            return new HttpResponseMessage
            {
                Content = new ObjectContent(
                    typeof(Me),
                    new Me
                    {
                        Details = identity.Claims
                            .Select(x => new Detail { Name = x.Type, Value = x.Value })
                            .ToList()
                    },
                    new JsonMediaTypeFormatter())
            };
        }
    }
}
