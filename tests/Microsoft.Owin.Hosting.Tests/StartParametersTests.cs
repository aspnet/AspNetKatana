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
            //var applicationBase = Path.GetDirectoryName(typeof(StartParametersTests).Assembly.Location);
            var applicationBase = Directory.GetCurrentDirectory();

            var info = new AppDomainSetup
            {
                ApplicationBase = applicationBase,
                PrivateBinPath = "bin",
                PrivateBinPathProbe = "*",
                ConfigurationFile = Path.Combine(applicationBase, "web.config")
            };
            var domain = AppDomain.CreateDomain("Test", null, info);

            try
            {
                var target = (SimpleTarget)domain.CreateInstanceFromAndUnwrap(
                    typeof(SimpleTarget).Assembly.Location,
                    typeof(SimpleTarget).FullName);

                var parameters = new StartParameters { Scheme = "alpha", Path = "/beta" };
                target.LoadWhenNeeded(applicationBase);
                var result = target.PassParameters(parameters);
                result.ShouldBe("alpha/beta");
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }
    }

    public class SimpleTarget : MarshalByRefObject
    {
        public string PassParameters(StartParameters parameters)
        {
            return parameters.Scheme + parameters.Path;
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
