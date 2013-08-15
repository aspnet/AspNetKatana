// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin;
using Owin.Loader.Tests;

[assembly: OwinStartup(typeof(Startup))]
[assembly: OwinStartup("AFriendlyName", typeof(Startup))]
[assembly: OwinStartup("AlternateConfiguration", typeof(Startup), "AlternateConfiguration")]

namespace Owin.Loader.Tests
{
    public class Startup
    {
        public static int ConfigurationCalls { get; set; }
        public static int AlternateConfigurationCalls { get; set; }

        public void Configuration(IAppBuilder builder)
        {
            ConfigurationCalls++;
        }

        public void AlternateConfiguration(IAppBuilder builder)
        {
            AlternateConfigurationCalls++;
        }
    }
}
