// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using LTAF.Infrastructure;
using Microsoft.Web.Administration;

namespace FunctionalTests.Common
{
    internal class WebDeployer : IDisposable
    {
        public WebServer WebServer { get; set; }

        public Application Application { get; set; }

        public string Deploy(string applicationName, KatanaWebConfiguration webConfig)
        {
            this.WebServer = WebServer.Create(GetWebServerType());
            string uniqueAppName = this.WebServer.DefaultWebSite.GetUniqueApplicaionName(applicationName);
            string appPhysicalPath = Path.Combine(this.WebServer.RootPhysicalPath, uniqueAppName);
            this.Application = this.WebServer.DefaultWebSite.Applications.Add("/" + uniqueAppName, appPhysicalPath);
            this.Application.Deploy(GetAssembliesInCurrentDirectory(), "bin");
            this.Application.Deploy("web.config", webConfig.GetWebConfigurationContent());
            this.WebServer.ServerManager.CommitChanges();
            return this.WebServer.DefaultWebSite.GetHttpVirtualPath() + this.Application.Path;
        }

        public void Dispose()
        {
            //var app = this.WebServer.DefaultWebSite.Applications[this.Application.Path];
            //this.WebServer.DefaultWebSite.Applications.Remove(app);
            //this.WebServer.ServerManager.CommitChanges();
        }

        #region Utilities
        private static bool webServerTypeInitialized = false;
        private static WebServerType _webServerType = WebServerType.IIS;
        public static WebServerType GetWebServerType()
        {
            if (!webServerTypeInitialized)
            {
                WebServerType webServerType;
                _webServerType = Enum.TryParse<WebServerType>(Environment.GetEnvironmentVariable("WebHost"), out webServerType) ? webServerType : WebServerType.IIS;
                webServerTypeInitialized = true;
            }

            return _webServerType;
        }

        private static string[] assembliesInCurrentDirectory;
        private static string[] GetAssembliesInCurrentDirectory()
        {
            if (assembliesInCurrentDirectory == null)
            {
                assembliesInCurrentDirectory = Directory.GetFiles(Environment.CurrentDirectory, "*.dll");
            }

            return assembliesInCurrentDirectory;
        }

        #endregion
    }
}
