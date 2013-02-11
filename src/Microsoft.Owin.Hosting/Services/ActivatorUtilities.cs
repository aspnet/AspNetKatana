// <copyright file="ActivatorUtilities.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Microsoft.Owin.Hosting.Services
{
    public static class ActivatorUtilities
    {
        public static object GetServiceOrCreateInstance(IServiceProvider services, Type type)
        {
            return GetServiceNoExceptions(services, type) ?? CreateInstance(services, type);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "IServiceProvider may throw unknown exceptions")]
        private static object GetServiceNoExceptions(IServiceProvider services, Type type)
        {
            try
            {
                return services.GetService(type);
            }
            catch
            {
                return null;
            }
        }

        public static object CreateInstance(IServiceProvider services, Type type)
        {
            return CreateFactory(type).Invoke(services);
        }

        public static Func<IServiceProvider, object> CreateFactory(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            ConstructorInfo[] constructors = type
                .GetConstructors()
                .Where(IsInjectable)
                .ToArray();

            if (constructors.Length == 1)
            {
                ParameterInfo[] parameters = constructors[0].GetParameters();
                return services =>
                {
                    var args = new object[parameters.Length];
                    for (int index = 0; index != parameters.Length; ++index)
                    {
                        args[index] = services.GetService(parameters[index].ParameterType);
                    }
                    return Activator.CreateInstance(type, args);
                };
            }
            return _ => Activator.CreateInstance(type);
        }

        private static bool IsInjectable(ConstructorInfo constructor)
        {
            return constructor.IsPublic && constructor.GetParameters().Length != 0;
        }
    }
}
