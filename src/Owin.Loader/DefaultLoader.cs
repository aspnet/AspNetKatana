// <copyright file="DefaultLoader.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2013 Microsoft Open Technologies, Inc. All rights reserved.
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Owin.Loader
{
    /// <summary>
    /// Locates the startup class based on the following convention:
    /// AssemblyName.Startup, with a method named Configuration
    /// </summary>
    internal class DefaultLoader
    {
        private readonly Func<string, Action<IAppBuilder>> _next;
        private readonly Func<Type, object> _activator;

        /// <summary>
        /// 
        /// </summary>
        public DefaultLoader()
        {
            _next = NullLoader.Instance;
            _activator = Activator.CreateInstance;
        }

        /// <summary>
        /// Allows for a fallback loader to be specified.
        /// </summary>
        /// <param name="next"></param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public DefaultLoader(Func<string, Action<IAppBuilder>> next)
        {
            _next = next ?? NullLoader.Instance;
            _activator = Activator.CreateInstance;
        }

        /// <summary>
        /// Allows for a fallback loader and a Dependency Injection activator to be specified.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="activator"></param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public DefaultLoader(Func<string, Action<IAppBuilder>> next, Func<Type, object> activator)
        {
            _next = next ?? NullLoader.Instance;
            _activator = activator;
        }

        /// <summary>
        /// Executes the loader, searching for the entry point by name.
        /// </summary>
        /// <param name="startupName">The name of the assembly and type entry point</param>
        /// <returns></returns>
        public Action<IAppBuilder> Load(string startupName)
        {
            return LoadImplementation(startupName) ?? _next(startupName);
        }

        private Action<IAppBuilder> LoadImplementation(string startupName)
        {
            if (string.IsNullOrWhiteSpace(startupName))
            {
                startupName = GetDefaultConfigurationString(
                    assembly => new[] { Constants.Startup, assembly.GetName().Name + "." + Constants.Startup });
            }

            var typeAndMethod = GetTypeAndMethodNameForConfigurationString(startupName);

            if (typeAndMethod == null)
            {
                return null;
            }

            var type = typeAndMethod.Item1;
            // default to the "Configuration" method if only the type name was provided
            var methodName = typeAndMethod.Item2 ?? Constants.Configuration;

            var startup = MakeDelegate(type, methodName);

            if (startup == null)
            {
                return null;
            }

            return
                builder =>
                {
                    if (builder == null)
                    {
                        throw new ArgumentNullException("builder");
                    }

                    object value;
                    if (!builder.Properties.TryGetValue(Constants.HostAppName, out value) ||
                        String.IsNullOrWhiteSpace(Convert.ToString(value, CultureInfo.InvariantCulture)))
                    {
                        builder.Properties[Constants.HostAppName] = type.FullName;
                    }
                    startup(builder);
                };
        }

        private static Tuple<Type, string> GetTypeAndMethodNameForConfigurationString(string configuration)
        {
            foreach (var hit in HuntForAssemblies(configuration))
            {
                var longestPossibleName = hit.Item1; // method or type name
                var assembly = hit.Item2;

                // try the longest 2 possibilities at most (because you can't have a dot in the method name)
                // so, typeName could specify a method or a type. we're looking for a type.
                foreach (var typeName in DotByDot(longestPossibleName).Take(2))
                {
                    var type = assembly.GetType(typeName, false);
                    if (type == null)
                    {
                        // must have been a method name (or doesn't exist), next!
                        continue;
                    }

                    var methodName = typeName == longestPossibleName
                        ? null
                        : longestPossibleName.Substring(typeName.Length + 1);

                    return new Tuple<Type, string>(type, methodName);
                }
            }
            return null;
        }

        // Scan the current directory and all private bin path subdirectories for the first managed assembly
        // with the given default type name.
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", 
            MessageId = "System.Reflection.Assembly.LoadFrom", Justification = "We are looking in specific directories for assemblies.")]
        private static string GetDefaultConfigurationString(Func<Assembly, string[]> defaultTypeNames)
        {
            var info = AppDomain.CurrentDomain.SetupInformation;

            IEnumerable<string> searchPaths = new string[0];
            if (info.PrivateBinPathProbe == null || string.IsNullOrWhiteSpace(info.PrivateBinPath))
            {
                // Check the current directory
                searchPaths = searchPaths.Concat(new string[] { string.Empty });
            }
            if (!string.IsNullOrWhiteSpace(info.PrivateBinPath))
            {
                // PrivateBinPath may be a semicolon separated list of subdirectories.
                searchPaths = searchPaths.Concat(info.PrivateBinPath.Split(';'));
            }

            foreach (string searchPath in searchPaths)
            {
                var assembliesPath = Path.Combine(info.ApplicationBase, searchPath);

                if (!Directory.Exists(assembliesPath))
                {
                    continue;
                }

                var files = Directory.GetFiles(assembliesPath, "*.dll")
                    .Concat(Directory.GetFiles(assembliesPath, "*.exe"));

                foreach (var file in files)
                {
                    Assembly assembly = null;

                    try
                    {
                        assembly = Assembly.Load(AssemblyName.GetAssemblyName(file));
                    }
                    catch (FileLoadException)
                    {
                        // "System.IO.FileLoadException: API restriction: The assembly 'foo.dll' has already loaded from a different 
                        // location. It cannot be loaded from a new location within the same appdomain."
                        // Infrastructure dlls may exist in multiple directories. Find the already loaded version.
                        string searchForAssemblyName = Path.GetFileNameWithoutExtension(file);
                        assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assemblyMatch =>
                            string.Equals(searchForAssemblyName, Path.GetFileNameWithoutExtension(assemblyMatch.Location), StringComparison.OrdinalIgnoreCase));
                    }
                    catch (BadImageFormatException)
                    {
                        // Not a managed dll/exe
                    }

                    if (assembly == null)
                    {
                        continue;
                    }

                    foreach (var possibleType in defaultTypeNames(assembly))
                    {
                        Type startupType = assembly.GetType(possibleType, false);
                        if (startupType != null)
                        {
                            // Verify this class has a public method Configuration, helps limit false positives.
                            if (startupType.GetMethods().Where(methodInfo => methodInfo.Name.Equals(Constants.Configuration)).Count() > 0)
                            {
                                return possibleType + ", " + assembly.FullName;
                            }
                        }
                    }
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

            var commaIndex = configurationString.IndexOf(',');
            if (commaIndex >= 0)
            {
                // assembly is given, break the type and assembly apart
                var methodOrTypeName = DotByDot(configurationString.Substring(0, commaIndex)).FirstOrDefault();
                var assemblyName = configurationString.Substring(commaIndex + 1).Trim();
                var assembly = TryAssemblyLoad(assemblyName);
                if (assembly != null)
                {
                    yield return Tuple.Create(methodOrTypeName, assembly);
                }
            }
            else
            {
                // assembly is inferred from type name
                var methodOrTypeName = DotByDot(configurationString).FirstOrDefault();

                // go through each segment
                foreach (var assemblyName in DotByDot(methodOrTypeName))
                {
                    var assembly = TryAssemblyLoad(assemblyName);
                    if (assembly != null)
                    {
                        if (assemblyName.Length == methodOrTypeName.Length)
                        {
                            // No type specified, use the default.
                            yield return Tuple.Create(assemblyName + "." + Constants.Startup, assembly);
                        }
                        else
                        {
                            yield return Tuple.Create(methodOrTypeName, assembly);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IEnumerable<string> DotByDot(string text)
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

        private Action<IAppBuilder> MakeDelegate(Type type, string methodName)
        {
            foreach (MethodInfo methodInfo in type.GetMethods())
            {
                if (!methodInfo.Name.Equals(methodName))
                {
                    continue;
                }

                if (Matches(methodInfo, typeof(void), typeof(IAppBuilder)))
                {
                    var instance = methodInfo.IsStatic ? null : _activator(type);
                    return builder => methodInfo.Invoke(instance, new[] { builder });
                }

                if (Matches(methodInfo, null, typeof(IDictionary<string, object>)))
                {
                    var instance = methodInfo.IsStatic ? null : _activator(type);
                    return builder => builder.Use(new Func<object, object>(_ => methodInfo.Invoke(instance, new object[] { builder.Properties })));
                }

                if (Matches(methodInfo, null))
                {
                    var instance = methodInfo.IsStatic ? null : _activator(type);
                    return builder => builder.Use(new Func<object, object>(_ => methodInfo.Invoke(instance, new object[] { builder.Properties })));
                }
            }

            return null;
        }

        private static bool Matches(MethodInfo methodInfo, Type returnType, params Type[] parameterTypes)
        {
            if (returnType != null && methodInfo.ReturnType != returnType)
            {
                return false;
            }

            var parameters = methodInfo.GetParameters();
            if (parameters.Length != parameterTypes.Length)
            {
                return false;
            }

            return parameters.Zip(parameterTypes, (pi, t) => pi.ParameterType == t).All(b => b);
        }
    }
}
