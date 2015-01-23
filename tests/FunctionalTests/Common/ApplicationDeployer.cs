// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Owin.Hosting;
using Owin;

namespace FunctionalTests.Common
{
    internal class ApplicationDeployer : IDisposable
    {
        private string DefaultSelfHostUrl_Http = "http://localhost:5000/";
        private const int SLEEP_AFTER_WEB_DEPLOY = 500;
        private string ApplicationUrl = null;

        public bool AutomaticAppStartupInWebHost = true;

        public IDisposable Application;

        public ApplicationDeployer()
        {
        }

        public ApplicationDeployer(string hostNameOverride)
        {
            this.DefaultSelfHostUrl_Http = new UriBuilder(this.DefaultSelfHostUrl_Http) { Host = hostNameOverride }.Uri.AbsoluteUri;
        }

        public string Deploy<T>(HostType hostType)
        {
            Trace.WriteLine(string.Format("Deploying {0} application in hostType = {1}", typeof(T).Name, hostType));

            switch (hostType)
            {
                case HostType.IIS:
                    var webDeployer = new WebDeployer();
                    var webConfig = new KatanaWebConfiguration() { StartupClass = typeof(T), AutomaticAppStartup = AutomaticAppStartupInWebHost };
                    ApplicationUrl = webDeployer.Deploy(typeof(T).Name, webConfig);
                    this.Application = webDeployer;
                    Thread.CurrentThread.Join(SLEEP_AFTER_WEB_DEPLOY);
                    break;
                case HostType.HttpListener:
                    ApplicationUrl = DefaultSelfHostUrl_Http;
                    this.Application = WebApp.Start<T>(DefaultSelfHostUrl_Http);
                    break;
                default:
                    throw new Exception("Unknown host type");
            }

            Trace.WriteLine(string.Format("Application successfully deployed to URL : {0}", ApplicationUrl));
            return ApplicationUrl;
        }

        public string Deploy(HostType hostType, Action<IAppBuilder> applicationStartup)
        {
            string startupMethodName = applicationStartup.GetFullyQualifiedConfigurationMethodName();
            Trace.WriteLine(string.Format("Deploying {0} application in hostType = {1}", startupMethodName, hostType));

            switch (hostType)
            {
                case HostType.IIS:
                    var webDeployer = new WebDeployer();
                    var webConfig = new KatanaWebConfiguration() { StartupMethod = startupMethodName, AutomaticAppStartup = AutomaticAppStartupInWebHost };
                    ApplicationUrl = webDeployer.Deploy(applicationStartup.GetApplicationName(), webConfig) + "/";
                    this.Application = webDeployer;
                    Thread.CurrentThread.Join(SLEEP_AFTER_WEB_DEPLOY);
                    break;
                case HostType.HttpListener:
                    ApplicationUrl = DefaultSelfHostUrl_Http;
                    this.Application = WebApp.Start(DefaultSelfHostUrl_Http, applicationStartup);
                    break;
                default:
                    throw new Exception("Unknown host type");
            }

            Trace.WriteLine(string.Format("Application successfully deployed to URL : {0}", ApplicationUrl));
            return ApplicationUrl;
        }

        public string Deploy(HostType hostType)
        {
            Trace.WriteLine(string.Format("Deploying application in hostType = {0}", hostType));

            switch (hostType)
            {
                case HostType.IIS:
                    var webDeployer = new WebDeployer();
                    var webConfig = new KatanaWebConfiguration() { AutomaticAppStartup = AutomaticAppStartupInWebHost };
                    ApplicationUrl = webDeployer.Deploy(System.Guid.NewGuid().ToString(), webConfig);
                    this.Application = webDeployer;
                    Thread.CurrentThread.Join(SLEEP_AFTER_WEB_DEPLOY);
                    break;
                case HostType.HttpListener:
                    ApplicationUrl = DefaultSelfHostUrl_Http;
                    this.Application = WebApp.Start(new StartOptions(DefaultSelfHostUrl_Http));
                    break;
                default:
                    throw new Exception("Unknown host type");
            }

            Trace.WriteLine(string.Format("Application successfully deployed to URL : {0}", ApplicationUrl));
            return ApplicationUrl;
        }

        public void Dispose()
        {
            if (this.Application != null)
            {
                this.Application.Dispose();
            }
        }
    }
}