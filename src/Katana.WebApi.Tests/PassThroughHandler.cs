using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Microsoft.AspNet.WebApi.Owin.Tests
{
    public class PassThroughHandler : DelegatingHandler
    {
        public PassThroughHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }
    }
}
