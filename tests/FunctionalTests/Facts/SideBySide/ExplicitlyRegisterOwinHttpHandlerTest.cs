// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Xml;
using FunctionalTests.Common;
using LTAF.Infrastructure;
using Owin;
using Xunit;

namespace FunctionalTests.Facts.SideBySide
{
    public class ExplicitlyRegisterOwinHttpHandlerTest
    {
        [Fact, Trait("FunctionalTests", "General")]
        public void ExplicitlyRegisterOwinHttpHandler()
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                deployer.AutomaticAppStartupInWebHost = false;
                string url = deployer.Deploy(HostType.IIS);
                var webConfigPath = deployer.GetWebConfigPath();

                var addHandler = "<system.webServer>" +
                                            "<handlers>" +
                                                "<add name=\"Owin\" verb=\"*\" type=\"Microsoft.Owin.Host.SystemWeb.OwinHttpHandler, Microsoft.Owin.Host.SystemWeb\" path=\"*\" />" +
                                            "</handlers>" +
                                        "</system.webServer>";

                var configuration = new XmlDocument() { InnerXml = File.ReadAllText(webConfigPath) };
                var configurationNode = configuration.SelectSingleNode("/configuration");
                configurationNode.InnerXml += addHandler;
                File.WriteAllText(webConfigPath, configuration.InnerXml);

                ((WebDeployer)deployer.Application).Application.Deploy("Default.aspx", File.ReadAllText("RequirementFiles\\Default.aspx"));

                Assert.Equal(Startup.RESULT, HttpClientUtility.GetResponseTextFromUrl(url + "/Default.aspx"));
                Assert.Equal(Startup.RESULT, HttpClientUtility.GetResponseTextFromUrl(url));
            }
        }
    }
}