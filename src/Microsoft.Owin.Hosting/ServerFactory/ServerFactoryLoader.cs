// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Owin.Hosting.ServerFactory
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
        /// - Assembly.Name. Look for type Assembly.Name.ServerFactory in the assembly Assembly.Name.
        /// - Assembly.Name.FactoryName.  Look for type Assembly.Name.FactoryName in the assembly Assembly.Name.
        /// </summary>
        /// <param name="serverName">The name of the assembly and type of the server factory</param>
        /// <returns></returns>
        public virtual IServerFactoryAdapter Load(string serverName)
        {
            if (string.IsNullOrWhiteSpace(serverName))
            {
                return null;
            }

            Type serverFactoryType = GetTypeAndMethodNameForConfigurationString(serverName);
            if (serverFactoryType == null)
            {
                return null;
            }

            return new ServerFactoryAdapter(serverFactoryType, _activator);
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
                            yield return Tuple.Create(assemblyName + "." + Constants.OwinServerFactory, assembly);
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
            for (int length = text.Length;
                length > 0;
                length = text.LastIndexOf('.', length - 1, length - 1))
            {
                yield return text.Substring(0, length);
            }
        }
    }
}
