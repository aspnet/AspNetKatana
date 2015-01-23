// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Web.Routing;
using FunctionalTests.Common;
using LTAF.Infrastructure;
using Owin;
using Xunit;

namespace FunctionalTests.Facts.SideBySide
{
    public class MapOwinRouteTest
    {
        [Fact, Trait("FunctionalTests", "General")]
        public void MapOwinRoute()
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string url = deployer.Deploy(HostType.IIS, MapOwinRouteConfiguration);
                ((WebDeployer)deployer.Application).Application.Deploy("Default.aspx", File.ReadAllText("RequirementFiles\\Default.aspx"));

                Assert.True(HttpClientUtility.GetResponseTextFromUrl(url + "/Default.aspx").Contains("Asp.net Test page"), "Default.aspx page not returned successfully in SxS mode");
                Assert.Equal(HttpClientUtility.GetResponseTextFromUrl(url + "/Route"), "Route");
                Assert.Equal(HttpClientUtility.GetResponseTextFromUrl(url + "/Route/Test"), "Route");
                Assert.Equal(HttpClientUtility.GetResponseTextFromUrl(url + "/RouteOne"), "RouteOne");
                Assert.Equal(HttpClientUtility.GetResponseTextFromUrl(url + "/RouteOne/Test"), "RouteOne");
                Assert.Equal(HttpClientUtility.GetResponseTextFromUrl(url + "/RouteTwo"), "RouteTwo");
                Assert.Equal(HttpClientUtility.GetResponseTextFromUrl(url + "/RouteTwo/Test"), "RouteTwo");
                Assert.Equal(HttpClientUtility.GetResponseTextFromUrl(url + "/RouteRouteTest"), "RouteRouteTest");
                Assert.Equal(HttpClientUtility.GetResponseTextFromUrl(url + "/RouteRouteTest/Test"), "RouteRouteTest");
            }
        }

        /// <summary>
        /// Asp.net routing. 
        /// SignalR or any partner teams should take dependency over this route extension to get the routes working.
        /// </summary>
        /// <param name="app"></param>
        public void MapOwinRouteConfiguration(IAppBuilder app)
        {
            RouteTable.Routes.MapOwinRoute("Route/{*operation}", builder => { builder.Use(typeof(Application), "Route"); });
            RouteTable.Routes.MapOwinRoute("RouteOne/{*operation}", builder => { builder.Use(typeof(Application), "RouteOne"); });
            RouteTable.Routes.MapOwinRoute("RouteTwo/{*operation}", builder => { builder.Use(typeof(Application), "RouteTwo"); });
            RouteTable.Routes.MapOwinRoute("RouteRouteTest/{*operation}", builder => { builder.Use(typeof(Application), "RouteRouteTest"); });
        }
    }
}
