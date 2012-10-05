//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace Katana.Server.AspNet.WebApplication
{
    public class HelloController : ApiController
    {
        public HttpResponseMessage Get(HttpRequestMessage request)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Hello Web API", Encoding.UTF8, "text/plain")
            };
        }
    }
}