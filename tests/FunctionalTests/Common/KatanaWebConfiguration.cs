// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Xml;

namespace FunctionalTests.Common
{
    internal class KatanaWebConfiguration
    {
        private const string AutomaticAppStartupXpath = "/configuration/appSettings/add[@key='owin:AutomaticAppStartup']";
        private const string AppStartupXpath = "/configuration/appSettings/add[@key='owin:AppStartup']";

        private static string webConfigTemplate = null;
        private static string WebConfigTemplate
        {
            get
            {
                if (webConfigTemplate == null)
                {
                    webConfigTemplate = File.ReadAllText("OwinWebConfigTemplate.txt");
                }

                return webConfigTemplate;
            }
        }

        /// <summary>
        /// On by default. Enables the Integrated pipeline mode. 
        /// </summary>
        public bool AutomaticAppStartup { get; set; }

        /// <summary>
        /// Entry point can be a class or a method
        /// </summary>
        public Type StartupClass { get; set; }

        /// <summary>
        /// Entry point can be a class or a method
        /// </summary>
        public string StartupMethod { get; set; }

        public KatanaWebConfiguration()
        {
            this.AutomaticAppStartup = true;
        }

        public string GetWebConfigurationContent()
        {
            var configuration = new XmlDocument();
            configuration.LoadXml(WebConfigTemplate);

            if (!AutomaticAppStartup)
            {
                var automaticStartupNode = configuration.SelectSingleNode(AutomaticAppStartupXpath);
                automaticStartupNode.Attributes["value"].Value = "false";
            }

            var startupConfiguration = configuration.SelectSingleNode(AppStartupXpath);
            if (StartupClass != null)
            {
                startupConfiguration.Attributes["value"].Value = StartupClass.AssemblyQualifiedName;
            }
            else if (StartupMethod != null)
            {
                startupConfiguration.Attributes["value"].Value = StartupMethod;
            }
            else
            {
                startupConfiguration.ParentNode.RemoveChild(startupConfiguration);
            }

            string configXml = null;
            using (var memoryStream = new MemoryStream())
            {
                var xmlWriter = XmlTextWriter.Create(memoryStream, new XmlWriterSettings() { Indent = true, ConformanceLevel = ConformanceLevel.Document });
                configuration.Save(xmlWriter);
                configXml = Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            return configXml;
        }
    }
}
