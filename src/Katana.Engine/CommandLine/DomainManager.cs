//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Katana.Engine.CommandLine
{
    public class DomainManager : AppDomainManager
    {
        public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
        {
            var defaultApplicationBase = appDomainInfo.ApplicationBase;
            var currentDirectory = Environment.CurrentDirectory;

            appDomainInfo.ApplicationBase = currentDirectory;
            appDomainInfo.PrivateBinPath = "bin";
            appDomainInfo.PrivateBinPathProbe = "*";
            appDomainInfo.ConfigurationFile = Path.Combine(currentDirectory, "web.config");

            ResolveAssembliesFromDirectory(defaultApplicationBase);
        }

        public static void ResolveAssembliesFromDirectory(string directory)
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

                    var shortName = new AssemblyName(b.Name).Name;
                    var path = Path.Combine(directory, shortName + ".dll");
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
