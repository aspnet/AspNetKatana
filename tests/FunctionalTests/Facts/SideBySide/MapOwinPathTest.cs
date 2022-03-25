// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;
using System.Web.Routing;
using FunctionalTests.Common;
using LTAF.Infrastructure;
using Microsoft.Owin;
using Owin;
using Xunit;

namespace FunctionalTests.Facts.SideBySide
{
    public class MapOwinPathTest
    {
        [Fact, Trait("FunctionalTests", "General")]
        public void MapOwinPath()
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string url = deployer.Deploy(HostType.IIS, MapOwinPathConfiguration);
                ((WebDeployer)deployer.Application).Application.Deploy("Default.aspx", File.ReadAllText("RequirementFiles\\Default.aspx"));

                Assert.True(HttpClientUtility.GetResponseTextFromUrl(url + "/Default.aspx").Contains("Asp.net Test page"), "Default.aspx page not returned successfully in SxS mode");
                Assert.Equal("prefix1", HttpClientUtility.GetResponseTextFromUrl(url + "/prefix1"));
                Assert.Equal("prefix1Append", HttpClientUtility.GetResponseTextFromUrl(url + "/prefix1Append"));
                Assert.Equal("prefix2", HttpClientUtility.GetResponseTextFromUrl(url + "/prefix2"));
            }
        }

        /// <summary>
        /// This does a match based on requestUrl.StartsWith(route). This is just for very primitive pattern matching
        /// if all different route paths are different. 
        /// </summary>
        /// <param name="app"></param>
        internal void MapOwinPathConfiguration(IAppBuilder app)
        {
            RouteTable.Routes.MapOwinPath("/prefix1", builder => { builder.Use(typeof(Application), "prefix1"); });
            RouteTable.Routes.MapOwinPath("/prefix1Append", builder => { builder.Use(typeof(Application), "prefix1Append"); });
            RouteTable.Routes.MapOwinPath("/prefix1/prefix2", builder => { builder.Use(typeof(Application), "prefix1/prefix2"); });
            RouteTable.Routes.MapOwinPath("/prefix2", builder => { builder.Use(typeof(Application), "prefix2"); });
        }
    }

    public class Application : OwinMiddleware
    {
        private string data;

        public Application(OwinMiddleware next, string data)
            : base(next)
        {
            this.data = data;
        }

        public override Task Invoke(IOwinContext context)
        {
            return context.Response.WriteAsync(data);
        }
    }
}
