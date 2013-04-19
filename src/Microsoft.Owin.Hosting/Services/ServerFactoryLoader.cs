// <copyright file="ServerFactoryLoader.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
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
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Owin.Hosting.Services
{
    /// <summary>
    /// Located and loads the server factory.
    /// </summary>
    public class ServerFactoryLoader : IServerFactoryLoader
    {
        private readonly IServerFactoryActivator _activator;

        /// <summary>
        /// Allows for a Dependency Injection activator to be specified.
        /// </summary>
        /// <param name="activator"></param>
        public ServerFactoryLoader(IServerFactoryActivator activator)
        {
            _activator = activator;
        }

        /// <summary>
        /// Executes the loader, searching for the server factory by name.
        /// Acceptable inputs:
        /// - null, empty, etc.. Scan for an assembly containing the type [Assembly.Name].ServerFactory.
        /// - Assembly.Name. Look for type Assembly.Name.ServerFactory in the assembly Assembly.Name.
        /// - Assembly.Name.FactoryName.  Look for type Assembly.Name.FactoryName in the assembly Assembly.Name.
        /// </summary>
        /// <param name="serverName">The name of the assembly and type of the server factory</param>
        /// <returns></returns>
        public virtual IServerFactoryAdapter Load(string serverName)
        {
            if (string.IsNullOrWhiteSpace(serverName))
            {
                serverName = GetDefaultConfigurationString(
                    assembly => new[] { "ServerFactory", assembly.GetName().Name + ".ServerFactory" });
            }

            Type serverFactoryType = GetTypeAndMethodNameForConfigurationString(serverName);
            if (serverFactoryType == null)
            {
                return null;
            }

            return new ServerFactoryAdapter(serverFactoryType, _activator);
        }

        // Scan the current directory and all private bin path sub-directories for the first managed assembly
        // with the given default type name.
        private static string GetDefaultConfigurationString(Func<Assembly, string[]> defaultTypeNames)
        {
            AppDomainSetup info = AppDomain.CurrentDomain.SetupInformation;

            IEnumerable<string> searchPaths = new string[0];
            if (info.PrivateBinPathProbe == null || string.IsNullOrWhiteSpace(info.PrivateBinPath))
            {
                // Check the current directory
                searchPaths = searchPaths.Concat(new string[] { string.Empty });
            }
            if (!string.IsNullOrWhiteSpace(info.PrivateBinPath))
            {
                // PrivateBinPath may be a semicolon separated list of sub-directories.
                searchPaths = searchPaths.Concat(info.PrivateBinPath.Split(';'));
            }

            foreach (string searchPath in searchPaths)
            {
                string assembliesPath = Path.Combine(info.ApplicationBase, searchPath);

                if (!Directory.Exists(assembliesPath))
                {
                    continue;
                }

                IEnumerable<string> files = Directory.GetFiles(assembliesPath, "*.dll")
                    .Concat(Directory.GetFiles(assembliesPath, "*.exe"));

                foreach (var file in files)
                {
                    try
                    {
                        Assembly reflectionOnlyAssembly = Assembly.ReflectionOnlyLoadFrom(file);

                        string assemblyFullName = reflectionOnlyAssembly.FullName;

                        foreach (var possibleType in defaultTypeNames(reflectionOnlyAssembly))
                        {
                            Type serverType = reflectionOnlyAssembly.GetType(possibleType, false);
                            if (serverType != null)
                            {
                                return possibleType + ", " + assemblyFullName;
                            }
                        }
                    }
                    catch (BadImageFormatException)
                    {
                        // Not a managed dll/exe
                    }
                }
            }

            return null;
        }

        private static Type GetTypeAndMethodNameForConfigurationString(string configuration)
        {
            foreach (var hit in HuntForAssemblies(configuration))
            {
                string longestPossibleName = hit.Item1; // type name
                Assembly assembly = hit.Item2;

                // try the longest 2 possibilities at most (because you can't have a dot in the method name)
                // so, typeName could specify a method or a type. we're looking for a type.
                foreach (var typeName in DotByDot(longestPossibleName).Take(2))
                {
                    Type type = assembly.GetType(typeName, false);
                    if (type == null)
                    {
                        // Doesn't exist? next!
                        continue;
                    }

                    return type;
                }
            }
            return null;
        }

        private static IEnumerable<Tuple<string, Assembly>> HuntForAssemblies(string configurationString)
        {
            if (configurationString == null)
            {
                yield break;
            }

            int commaIndex = configurationString.IndexOf(',');
            if (commaIndex >= 0)
            {
                // assembly is given, break the type and assembly apart
                string methodOrTypeName = DotByDot(configurationString.Substring(0, commaIndex)).FirstOrDefault();
                string assemblyName = configurationString.Substring(commaIndex + 1).Trim();
                Assembly assembly = TryAssemblyLoad(assemblyName);
                if (assembly != null)
                {
                    yield return Tuple.Create(methodOrTypeName, assembly);
                }
            }
            else
            {
                // assembly is inferred from type name
                string typeName = DotByDot(configurationString).FirstOrDefault();

                // go through each segment
                foreach (var assemblyName in DotByDot(typeName))
                {
                    Assembly assembly = TryAssemblyLoad(assemblyName);
                    if (assembly != null)
                    {
                        if (assemblyName.Length == typeName.Length)
                        {
                            // No type specified, use the default.
                            yield return Tuple.Create(assemblyName + ".ServerFactory", assembly);
                        }
                        else
                        {
                            yield return Tuple.Create(typeName, assembly);
                        }
                    }
                }
            }
        }

        private static Assembly TryAssemblyLoad(string assemblyName)
        {
            try
            {
                return Assembly.Load(assemblyName);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        private static IEnumerable<string> DotByDot(string text)
        {
            if (text == null)
            {
                yield break;
            }

            text = text.Trim('.');
            for (var length = text.Length;
                length > 0;
                length = text.LastIndexOf('.', length - 1, length - 1))
            {
                yield return text.Substring(0, length);
            }
        }
    }
}
