// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http;
using System.Xml;
using FunctionalTests.Common;
using Microsoft.Owin;
using Owin;
using Xunit;
using Xunit.Extensions;

[assembly: OwinStartup("FriendlyClassName", typeof(FunctionalTests.Facts.OwinHost.StartOptionsAndOwinHost))]
[assembly: OwinStartup("FriendlyClassNameWithMethodNameOverride", typeof(FunctionalTests.Facts.OwinHost.StartOptionsAndOwinHost), "CopyOfConfiguration")]
[assembly: FunctionalTests.Facts.OwinHost.OwinStartup("CustomStartupAttribute", typeof(FunctionalTests.Facts.OwinHost.StartOptionsAndOwinHost))]

namespace FunctionalTests.Facts.OwinHost
{
    public partial class StartOptionsAndOwinHost
    {
        [Fact, Trait("FunctionalTests", "General")]
        public void ReadAppSettings()
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(HostType.IIS, Configuration);
                var vDirPath = Path.GetDirectoryName(deployer.GetWebConfigPath());
                var options = new MyStartOptions(true) { DontPassStartupClassInCommandLine = true, TargetApplicationDirectory = vDirPath };
                string webConfigPath = deployer.GetWebConfigPath();

                XmlDocument configuration = new XmlDocument() { InnerXml = File.ReadAllText(webConfigPath) };
                var appSettingsNode = configuration.SelectSingleNode("/configuration/appSettings");
                appSettingsNode.InnerXml += "<add key=\"traceoutput\" value=\"MyLogTextThroughAppSetting.txt\" />";
                File.WriteAllText(webConfigPath, configuration.InnerXml);

                using (new HostServer(options))
                {
                    var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Add("outputFile", "Test logging");
                    string response = httpClient.GetAsync("http://localhost:5000/").Result.Content.ReadAsStringAsync().Result;

                    Assert.Equal("SUCCESS", response);
                    Assert.True(File.Exists("MyLogTextThroughAppSetting.txt"), "Log file MyLogTextThroughAppSetting.txt is not created on specifying through appSetting");
                }
            }
        }

        [Theory, Trait("FunctionalTests", "General")]
        [InlineData("FriendlyClassName", "SUCCESS")]
        [InlineData("FriendlyClassNameWithMethodNameOverride", "CopyOfConfiguration")]
        [InlineData("CustomStartupAttribute", "SUCCESS")]
        //Test the case sensitivity of friendly names
        [InlineData("friendlyclassname", "SUCCESS")]
        public void FriendlyStartupNames(string friendlyAppStartupName, string expectedResponse)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(HostType.IIS, Configuration);
                var vDirPath = Path.GetDirectoryName(deployer.GetWebConfigPath());
                var options = new MyStartOptions(true) { TargetApplicationDirectory = vDirPath, FriendlyAppStartupName = friendlyAppStartupName };

                using (new HostServer(options))
                {
                    string response = HttpClientUtility.GetResponseTextFromUrl("http://localhost:5000/");
                    Assert.Equal(expectedResponse, response);
                }
            }
        }

        public void CopyOfConfiguration(IAppBuilder app)
        {
            app.Run((context) =>
                {
                    return context.Response.WriteAsync("CopyOfConfiguration");
                });
        }

        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.IIS)]
        public void FriendlyStartupNameInAppSetting(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType);

                //Tweek the web.config appSettings to say owin:AppStartup = FriendlyName
                var webConfigPath = deployer.GetWebConfigPath();

                XmlDocument configuration = new XmlDocument() { InnerXml = File.ReadAllText(webConfigPath) };
                var appSettingsNode = configuration.SelectSingleNode("/configuration/appSettings");
                appSettingsNode.InnerXml += "<add key=\"owin:AppStartup\" value=\"FriendlyClassNameWithMethodNameOverride\" />";
                File.WriteAllText(webConfigPath, configuration.InnerXml);

                string responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl);
                Assert.Equal("CopyOfConfiguration", responseText);
            }
        }
    }
}