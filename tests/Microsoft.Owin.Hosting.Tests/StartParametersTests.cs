// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Hosting.Tests
{
    public class StartParametersTests
    {
        [Fact]
        public void ParametersCanSerializeBetweenDomainsWithDifferentHostingAssemblies()
        {
            // var applicationBase = Path.GetDirectoryName(typeof(StartParametersTests).Assembly.Location);
            string applicationBase = Directory.GetCurrentDirectory();

            var info = new AppDomainSetup
            {
                ApplicationBase = applicationBase,
                PrivateBinPath = "bin",
                PrivateBinPathProbe = "*",
                ConfigurationFile = Path.Combine(applicationBase, "web.config")
            };
            AppDomain domain = AppDomain.CreateDomain("Test", null, info);

            try
            {
                var target = (SimpleTarget)domain.CreateInstanceFromAndUnwrap(
                    typeof(SimpleTarget).Assembly.Location,
                    typeof(SimpleTarget).FullName);

                target.LoadWhenNeeded(applicationBase);
                var options = new StartOptions("alpha://localhost/beta")
                {
                    AppStartup = "x",
                };
                options.Settings.Add("1", "2");
                string result = target.PassParameters(options);
                result.ShouldBe("alpha://localhost/betax2");
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        public class SimpleTarget : MarshalByRefObject
        {
            public string PassParameters(StartOptions options)
            {
                return options.Urls[0] + options.AppStartup + options.Settings["1"];
            }

            public void LoadWhenNeeded(string directory)
            {
                var cache = new Dictionary<string, Assembly>();
                AppDomain.CurrentDomain.AssemblyResolve +=
                    (a, b) =>
                    {
                        Assembly assembly;
                        if (cache.TryGetValue(b.Name, out assembly))
                        {
                            return assembly;
                        }

                        string shortName = new AssemblyName(b.Name).Name;
                        string path = Path.Combine(directory, shortName + ".dll");
                        if (File.Exists(path))
                        {
                            assembly = Assembly.LoadFile(path);
                        }
                        cache[b.Name] = assembly;
                        if (assembly != null)
                        {
                            cache[assembly.FullName] = assembly;
                        }
                        return assembly;
                    };
            }
        }
    }
}
