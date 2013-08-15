// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using SharedResourceNamespace;

namespace Owin.Loader
{
    using AppLoader = Func<string, IList<string>, Action<IAppBuilder>>;

    /// <summary>
    /// Locates the startup class based on the following convention:
    /// AssemblyName.Startup, with a method named Configuration
    /// </summary>
    internal class DefaultLoader
    {
        private readonly AppLoader _next;
        private readonly Func<Type, object> _activator;
        private readonly IEnumerable<Assembly> _referencedAssemblies;

        /// <summary>
        /// 
        /// </summary>
        public DefaultLoader()
            : this(null, null, null)
        {
        }

        public DefaultLoader(IEnumerable<Assembly> referencedAssemblies)
            : this(null, null, referencedAssemblies)
        {
        }

        /// <summary>
        /// Allows for a fallback loader to be specified.
        /// </summary>
        /// <param name="next"></param>
        public DefaultLoader(AppLoader next)
            : this(next, null, null)
        {
        }

        /// <summary>
        /// Allows for a fallback loader and a Dependency Injection activator to be specified.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="activator"></param>
        public DefaultLoader(AppLoader next, Func<Type, object> activator)
            : this(next, activator, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="activator"></param>
        /// <param name="referencedAssemblies"></param>
        public DefaultLoader(AppLoader next, Func<Type, object> activator,
            IEnumerable<Assembly> referencedAssemblies)
        {
            _next = next ?? NullLoader.Instance;
            _activator = activator ?? Activator.CreateInstance;
            _referencedAssemblies = referencedAssemblies ?? new AssemblyDirScanner();
        }

        /// <summary>
        /// Executes the loader, searching for the entry point by name.
        /// </summary>
        /// <param name="startupName">The name of the assembly and type entry point</param>
        /// <param name="errorDetails"></param>
        /// <returns></returns>
        public Action<IAppBuilder> Load(string startupName, IList<string> errorDetails)
        {
            return LoadImplementation(startupName, errorDetails) ?? _next(startupName, errorDetails);
        }

        private Action<IAppBuilder> LoadImplementation(string startupName, IList<string> errorDetails)
        {
            Tuple<Type, string> typeAndMethod = null;
            startupName = startupName ?? string.Empty;
            // Auto-discovery or Friendly name?
            if (!startupName.Contains(','))
            {
                typeAndMethod = GetDefaultConfiguration(startupName, errorDetails);
            }

            if (typeAndMethod == null && !string.IsNullOrWhiteSpace(startupName))
            {
                typeAndMethod = GetTypeAndMethodNameForConfigurationString(startupName, errorDetails);
            }

            if (typeAndMethod == null)
            {
                return null;
            }

            Type type = typeAndMethod.Item1;
            // default to the "Configuration" method if only the type name was provided
            string methodName = !string.IsNullOrWhiteSpace(typeAndMethod.Item2) ? typeAndMethod.Item2 : Constants.Configuration;

            Action<IAppBuilder> startup = MakeDelegate(type, methodName, errorDetails);

            if (startup == null)
            {
                return null;
            }

            return builder =>
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

        private Tuple<Type, string> GetTypeAndMethodNameForConfigurationString(string configuration, IList<string> errors)
        {
            Tuple<string, Assembly> typePair = HuntForAssembly(configuration, errors);
            if (typePair == null)
            {
                return null;
            }

            string longestPossibleName = typePair.Item1; // method or type name
            Assembly assembly = typePair.Item2;

            // try the longest 2 possibilities at most (because you can't have a dot in the method name)
            // so, typeName could specify a method or a type. we're looking for a type.
            foreach (var typeName in DotByDot(longestPossibleName).Take(2))
            {
                Type type = assembly.GetType(typeName, false);
                if (type == null)
                {
                    errors.Add(string.Format(CultureInfo.CurrentCulture, LoaderResources.ClassNotFoundInAssembly,
                        configuration, typeName, assembly.FullName));
                    // must have been a method name (or doesn't exist), next!
                    continue;
                }

                string methodName = typeName == longestPossibleName
                    ? null
                    : longestPossibleName.Substring(typeName.Length + 1);

                return new Tuple<Type, string>(type, methodName);
            }

            return null;
        }

        private Tuple<Type, string> GetDefaultConfiguration(string friendlyName, IList<string> errors)
        {
            friendlyName = friendlyName ?? string.Empty;
            bool conflict = false;
            Tuple<Type, string> result = SearchForStartupAttribute(friendlyName, errors, ref conflict);

            if (result == null && !conflict && string.IsNullOrEmpty(friendlyName))
            {
                result = SearchForStartupConvention(errors);
            }

            return result;
        }

        // Search for any assemblies with an OwinStartupAttribute. If a friendly name is provided, only accept an
        // attribute with the matching value.
        private Tuple<Type, string> SearchForStartupAttribute(string friendlyName, IList<string> errors, ref bool conflict)
        {
            friendlyName = friendlyName ?? string.Empty;
            bool foundAnyInstances = false;
            Tuple<Type, string> fullMatch = null;
            Assembly matchedAssembly = null;
            foreach (var assembly in _referencedAssemblies)
            {
                foreach (var owinStartupAttribute in assembly.GetCustomAttributes(inherit: false)
                                                             .Where(attribute => attribute.GetType().Name.Equals(Constants.OwinStartupAttribute, StringComparison.Ordinal)))
                {
                    Type attributeType = owinStartupAttribute.GetType();
                    foundAnyInstances = true;

                    // Find the StartupType property.
                    PropertyInfo startupTypeProperty = attributeType.GetProperty(Constants.StartupType, typeof(Type));
                    if (startupTypeProperty == null)
                    {
                        errors.Add(string.Format(CultureInfo.CurrentCulture, LoaderResources.StartupTypePropertyMissing,
                            attributeType.AssemblyQualifiedName, assembly.FullName));
                        continue;
                    }

                    var startupType = startupTypeProperty.GetValue(owinStartupAttribute, null) as Type;
                    if (startupType == null)
                    {
                        errors.Add(string.Format(CultureInfo.CurrentCulture, LoaderResources.StartupTypePropertyEmpty, assembly.FullName));
                        continue;
                    }

                    // FriendlyName is an optional property.
                    string friendlyNameValue = string.Empty;
                    PropertyInfo friendlyNameProperty = attributeType.GetProperty(Constants.FriendlyName, typeof(string));
                    if (friendlyNameProperty != null)
                    {
                        friendlyNameValue = friendlyNameProperty.GetValue(owinStartupAttribute, null) as string ?? string.Empty;
                    }

                    if (!string.Equals(friendlyName, friendlyNameValue, StringComparison.Ordinal))
                    {
                        errors.Add(string.Format(CultureInfo.CurrentCulture, LoaderResources.FriendlyNameMismatch,
                            friendlyNameValue, friendlyName, assembly.FullName));
                        continue;
                    }

                    // MethodName is an optional property.
                    string methodName = string.Empty;
                    PropertyInfo methodNameProperty = attributeType.GetProperty(Constants.MethodName, typeof(string));
                    if (methodNameProperty != null)
                    {
                        methodName = methodNameProperty.GetValue(owinStartupAttribute, null) as string ?? string.Empty;
                    }

                    if (fullMatch != null)
                    {
                        conflict = true;
                        errors.Add(string.Format(CultureInfo.CurrentCulture,
                            LoaderResources.Exception_AttributeNameConflict,
                            matchedAssembly.GetName().Name, fullMatch.Item1, assembly.GetName().Name, startupType, friendlyName));
                    }
                    else
                    {
                        fullMatch = new Tuple<Type, string>(startupType, methodName);
                        matchedAssembly = assembly;
                    }
                }
            }

            if (!foundAnyInstances)
            {
                errors.Add(LoaderResources.NoOwinStartupAttribute);
            }
            if (conflict)
            {
                return null;
            }
            return fullMatch;
        }

        // Search for any assemblies with a Startup or [AssemblyName].Startup class.
        private Tuple<Type, string> SearchForStartupConvention(IList<string> errors)
        {
            Type matchedType = null;
            bool conflict = false;
            foreach (var assembly in _referencedAssemblies)
            {
                // Startup
                CheckForStartupType(Constants.Startup, assembly, ref matchedType, ref conflict, errors);

                // [AssemblyName].Startup
                CheckForStartupType(assembly.GetName().Name + "." + Constants.Startup, assembly, ref matchedType, ref conflict, errors);
            }

            if (matchedType == null)
            {
                errors.Add(LoaderResources.NoAssemblyWithStartupClass);
                return null;
            }

            if (conflict)
            {
                return null;
            }

            // Verify this class has a public method Configuration, helps limit false positives.
            if (!matchedType.GetMethods().Any(methodInfo => methodInfo.Name.Equals(Constants.Configuration)))
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture,
                    LoaderResources.MethodNotFoundInClass, Constants.Configuration, matchedType.AssemblyQualifiedName));
                return null;
            }

            return new Tuple<Type, string>(matchedType, Constants.Configuration);
        }

        private static void CheckForStartupType(string startupName, Assembly assembly, ref Type matchedType, ref bool conflict, IList<string> errors)
        {
            Type startupType = assembly.GetType(startupName, throwOnError: false);
            if (startupType != null)
            {
                // Conflict?
                if (matchedType != null)
                {
                    conflict = true;
                    errors.Add(string.Format(CultureInfo.CurrentCulture,
                        LoaderResources.Exception_StartupTypeConflict,
                        matchedType.AssemblyQualifiedName, startupType.AssemblyQualifiedName));
                }
                else
                {
                    matchedType = startupType;
                }
            }
        }

        private Tuple<string, Assembly> HuntForAssembly(string configuration, IList<string> errors)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            int commaIndex = configuration.IndexOf(',');
            if (commaIndex >= 0)
            {
                // assembly is given, break the type and assembly apart
                string methodOrTypeName = DotByDot(configuration.Substring(0, commaIndex)).FirstOrDefault();
                string assemblyName = configuration.Substring(commaIndex + 1).Trim();
                Assembly assembly = TryAssemblyLoad(assemblyName);

                if (assembly == null)
                {
                    errors.Add(string.Format(CultureInfo.CurrentCulture, LoaderResources.AssemblyNotFound,
                        configuration, assemblyName));
                    return null;
                }
                return Tuple.Create(methodOrTypeName, assembly);
            }

            // See if any referenced assemblies contain this type
            foreach (var assembly in _referencedAssemblies)
            {
                // NameSpace.Type or NameSpace.Type.Method
                foreach (var typeName in DotByDot(configuration).Take(2))
                {
                    if (assembly.GetType(typeName, throwOnError: false) != null)
                    {
                        return Tuple.Create(configuration, assembly);
                    }
                }
            }

            errors.Add(string.Format(CultureInfo.CurrentCulture, LoaderResources.TypeOrMethodNotFound, configuration));
            return null;
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
            catch (FileLoadException)
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
            for (int length = text.Length;
                length > 0;
                length = text.LastIndexOf('.', length - 1, length - 1))
            {
                yield return text.Substring(0, length);
            }
        }

        private Action<IAppBuilder> MakeDelegate(Type type, string methodName, IList<string> errors)
        {
            MethodInfo partialMatch = null;
            foreach (var methodInfo in type.GetMethods())
            {
                if (!methodInfo.Name.Equals(methodName))
                {
                    continue;
                }

                // void Configuration(IAppBuilder app)
                if (Matches(methodInfo, false, typeof(IAppBuilder)))
                {
                    object instance = methodInfo.IsStatic ? null : _activator(type);
                    return builder => methodInfo.Invoke(instance, new[] { builder });
                }

                // object Configuration(IDictionary<string, object> appProperties)
                if (Matches(methodInfo, true, typeof(IDictionary<string, object>)))
                {
                    object instance = methodInfo.IsStatic ? null : _activator(type);
                    return builder => builder.Use(new Func<object, object>(_ => methodInfo.Invoke(instance, new object[] { builder.Properties })));
                }

                // object Configuration()
                if (Matches(methodInfo, true))
                {
                    object instance = methodInfo.IsStatic ? null : _activator(type);
                    return builder => builder.Use(new Func<object, object>(_ => methodInfo.Invoke(instance, new object[0])));
                }

                partialMatch = partialMatch ?? methodInfo;
            }

            if (partialMatch == null)
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture,
                    LoaderResources.MethodNotFoundInClass, methodName, type.AssemblyQualifiedName));
            }
            else
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture, LoaderResources.UnexpectedMethodSignature,
                    methodName, type.AssemblyQualifiedName));
            }
            return null;
        }

        private static bool Matches(MethodInfo methodInfo, bool hasReturnValue, params Type[] parameterTypes)
        {
            bool methodHadReturnValue = methodInfo.ReturnType != typeof(void);
            if (hasReturnValue != methodHadReturnValue)
            {
                return false;
            }

            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters.Length != parameterTypes.Length)
            {
                return false;
            }

            return parameters.Zip(parameterTypes, (pi, t) => pi.ParameterType == t).All(b => b);
        }

        private class AssemblyDirScanner : IEnumerable<Assembly>
        {
            public IEnumerator<Assembly> GetEnumerator()
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
                    // PrivateBinPath may be a semicolon separated list of subdirectories.
                    searchPaths = searchPaths.Concat(info.PrivateBinPath.Split(';'));
                }

                foreach (var searchPath in searchPaths)
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
                        Assembly assembly = null;

                        try
                        {
                            assembly = Assembly.Load(AssemblyName.GetAssemblyName(file));
                        }
                        catch (BadImageFormatException)
                        {
                            // Not a managed dll/exe
                            continue;
                        }

                        yield return assembly;
                    }
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
