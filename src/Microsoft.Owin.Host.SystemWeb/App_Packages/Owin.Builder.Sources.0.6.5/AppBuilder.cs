// Licensed to Monkey Square, Inc. under one or more contributor 
// license agreements.  See the NOTICE file distributed with 
// this work or additional information regarding copyright 
// ownership.  Monkey Square, Inc. licenses this file to you 
// under the Apache License, Version 2.0 (the "License"); you 
// may not use this file except in compliance with the License.
// You may obtain a copy of the License at 
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Owin.Builder
{
    // <summary>
    // A standard implementation of IAppBuilder 
    // </summary>
    internal class AppBuilder : IAppBuilder
    {
        private readonly IList<Tuple<Type, Delegate, object[]>> _middleware;
        private readonly IDictionary<Tuple<Type, Type>, Delegate> _conversions;
        private readonly IDictionary<string, object> _properties;

        public AppBuilder()
        {
            _properties = new Dictionary<string, object>();
            _conversions = new Dictionary<Tuple<Type, Type>, Delegate>();
            _middleware = new List<Tuple<Type, Delegate, object[]>>();

            _properties["builder.AddSignatureConversion"] = new Action<Delegate>(AddSignatureConversion);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public AppBuilder(
            IDictionary<Tuple<Type, Type>, Delegate> conversions,
            IDictionary<string, object> properties)
        {
            _properties = properties;
            _conversions = conversions;
            _middleware = new List<Tuple<Type, Delegate, object[]>>();
        }

        public IDictionary<string, object> Properties
        {
            get { return _properties; }
        }

        public IAppBuilder Use(object middleware, params object[] args)
        {
            _middleware.Add(ToMiddlewareFactory(middleware, args));
            return this;
        }

        public IAppBuilder New()
        {
            return new AppBuilder(_conversions, _properties);
        }

        public object Build(Type returnType)
        {
            return BuildInternal(returnType);
        }

        private void AddSignatureConversion(Delegate conversion)
        {
            if (conversion == null)
            {
                throw new ArgumentNullException("conversion");
            }

            Type parameterType = GetParameterType(conversion);
            if (parameterType == null)
            {
                throw new ArgumentException("Conversion delegate must take one parameter", "conversion");
            }
            Tuple<Type, Type> key = Tuple.Create(conversion.Method.ReturnType, parameterType);
            _conversions[key] = conversion;
        }

        private static Type GetParameterType(Delegate function)
        {
            ParameterInfo[] parameters = function.Method.GetParameters();
            return parameters.Length == 1 ? parameters[0].ParameterType : null;
        }

        private object BuildInternal(Type signature)
        {
            object app;
            if (!_properties.TryGetValue("builder.DefaultApp", out app))
            {
                app = new Func<IDictionary<string, object>, Task>(new NotFound().Invoke);
            }

            foreach (Tuple<Type, Delegate, object[]> middleware in _middleware.Reverse())
            {
                Type neededSignature = middleware.Item1;
                Delegate middlewareDelegate = middleware.Item2;
                object[] middlewareArgs = middleware.Item3;

                app = Convert(neededSignature, app);
                object[] invokeParameters = new[] { app }.Concat(middlewareArgs).ToArray();
                app = middlewareDelegate.DynamicInvoke(invokeParameters);
                app = Convert(neededSignature, app);
            }

            return Convert(signature, app);
        }

        private object Convert(Type signature, object app)
        {
            if (app == null)
            {
                return null;
            }

            object oneHop = ConvertOneHop(signature, app);
            if (oneHop != null)
            {
                return oneHop;
            }

            object multiHop = ConvertMultiHop(signature, app);
            if (multiHop != null)
            {
                return multiHop;
            }
            throw new ArgumentException(
                string.Format(CultureInfo.CurrentCulture, "No conversion available between {0} and {1}", app.GetType(), signature), 
                "signature");
        }

        private object ConvertMultiHop(Type signature, object app)
        {
            foreach (KeyValuePair<Tuple<Type, Type>, Delegate> conversion in _conversions)
            {
                object preConversion = ConvertOneHop(conversion.Key.Item2, app);
                if (preConversion == null)
                {
                    continue;
                }
                object intermediate = conversion.Value.DynamicInvoke(preConversion);
                if (intermediate == null)
                {
                    continue;
                }
                object postConversion = ConvertOneHop(signature, intermediate);
                if (postConversion == null)
                {
                    continue;
                }

                return postConversion;
            }
            return null;
        }

        private object ConvertOneHop(Type signature, object app)
        {
            if (signature.IsInstanceOfType(app))
            {
                return app;
            }
            if (typeof(Delegate).IsAssignableFrom(signature))
            {
                Delegate memberDelegate = ToMemberDelegate(signature, app);
                if (memberDelegate != null)
                {
                    return memberDelegate;
                }
            }
            foreach (KeyValuePair<Tuple<Type, Type>, Delegate> conversion in _conversions)
            {
                Type returnType = conversion.Key.Item1;
                Type parameterType = conversion.Key.Item2;
                if (parameterType.IsInstanceOfType(app) &&
                    signature.IsAssignableFrom(returnType))
                {
                    return conversion.Value.DynamicInvoke(app);
                }
            }
            return null;
        }

        private static Delegate ToMemberDelegate(Type signature, object app)
        {
            MethodInfo signatureMethod = signature.GetMethod("Invoke");
            ParameterInfo[] signatureParameters = signatureMethod.GetParameters();

            MethodInfo[] methods = app.GetType().GetMethods();
            foreach (MethodInfo method in methods)
            {
                if (method.Name != "Invoke")
                {
                    continue;
                }
                ParameterInfo[] methodParameters = method.GetParameters();
                if (methodParameters.Length != signatureParameters.Length)
                {
                    continue;
                }
                if (methodParameters
                    .Zip(signatureParameters, (methodParameter, signatureParameter) => methodParameter.ParameterType.IsAssignableFrom(signatureParameter.ParameterType))
                    .Any(compatible => compatible == false))
                {
                    continue;
                }
                if (!signatureMethod.ReturnType.IsAssignableFrom(method.ReturnType))
                {
                    continue;
                }
                return Delegate.CreateDelegate(signature, app, method);
            }
            return null;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "False positive")]
        private static Tuple<Type, Delegate, object[]> ToMiddlewareFactory(object middlewareObject, object[] args)
        {
            Delegate middlewareDelegate = middlewareObject as Delegate;
            if (middlewareDelegate == null)
            {
                MethodInfo[] methods = middlewareObject.GetType().GetMethods();
                foreach (MethodInfo method in methods)
                {
                    if (method.Name != "Invoke")
                    {
                        continue;
                    }
                    ParameterInfo[] parameters = method.GetParameters();
                    Type[] parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

                    if (parameterTypes.Length != args.Length + 1)
                    {
                        continue;
                    }
                    if (!parameterTypes
                        .Skip(1)
                        .Zip(args, TestArgForParameter)
                        .All(x => x))
                    {
                        continue;
                    }
                    IEnumerable<Type> genericFuncTypes = parameterTypes.Concat(new[] { method.ReturnType });
                    Type funcType = Expression.GetFuncType(genericFuncTypes.ToArray());
                    middlewareDelegate = Delegate.CreateDelegate(funcType, middlewareObject, method);
                    return Tuple.Create(parameters[0].ParameterType, middlewareDelegate, args);
                }
            }

            if (middlewareDelegate == null && middlewareObject is Type)
            {
                Type middlewareType = middlewareObject as Type;
                ConstructorInfo[] constructors = middlewareType.GetConstructors();
                foreach (ConstructorInfo constructor in constructors)
                {
                    ParameterInfo[] parameters = constructor.GetParameters();
                    Type[] parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
                    if (parameterTypes.Length != args.Length + 1)
                    {
                        continue;
                    }
                    if (!parameterTypes
                        .Skip(1)
                        .Zip(args, TestArgForParameter)
                        .All(x => x))
                    {
                        continue;
                    }

                    ParameterExpression[] parameterExpressions = parameters.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
                    NewExpression callConstructor = Expression.New(constructor, parameterExpressions);
                    middlewareDelegate = Expression.Lambda(callConstructor, parameterExpressions).Compile();
                    return Tuple.Create(parameters[0].ParameterType, middlewareDelegate, args);
                }

                if (middlewareDelegate == null)
                {
                    throw new MissingMethodException(middlewareType.FullName, "ctor(" + (args.Length + 1) + ")");
                }
            }

            if (middlewareDelegate == null)
            {
                throw new NotSupportedException((middlewareObject ?? string.Empty).ToString());
            }

            return Tuple.Create(GetParameterType(middlewareDelegate), middlewareDelegate, args);
        }

        private static bool TestArgForParameter(Type parameterType, object arg)
        {
            return (arg == null && !parameterType.IsValueType) ||
                parameterType.IsInstanceOfType(arg);
        }
    }
}
