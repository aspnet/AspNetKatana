// <copyright file="StartParametersTests.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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

                var parameters = new StartOptions { Scheme = "alpha", Path = "/beta" };
                target.LoadWhenNeeded(applicationBase);
                string result = target.PassParameters(parameters);
                result.ShouldBe("alpha/beta");
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
                return options.Scheme + options.Path;
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
