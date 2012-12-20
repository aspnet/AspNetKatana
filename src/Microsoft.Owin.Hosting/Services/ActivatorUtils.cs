using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Owin.Hosting.Services
{
    public static class ActivatorUtils
    {
        public static object CreateInstance(IServiceProvider services, Type type)
        {
            return CreateFactory(type).Invoke(services);
        }

        public static Func<IServiceProvider, object> CreateFactory(Type type)
        {
            var constructors = type
                .GetConstructors()
                .Where(IsInjectable)
                .ToArray();

            if (constructors.Length == 1)
            {
                var parameters = constructors[0].GetParameters();
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
