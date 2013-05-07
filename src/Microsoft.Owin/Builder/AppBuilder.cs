// <copyright file="AppBuilder.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Owin.Infrastructure;
using Owin;

namespace Microsoft.Owin.Builder
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// A standard implementation of IAppBuilder 
    /// </summary>
    public class AppBuilder : IAppBuilder
    {
        private static readonly AppFunc NotFound = new NotFound().Invoke;

        private readonly IList<Tuple<Type, Delegate, object[]>> _middleware;
        private readonly IDictionary<Tuple<Type, Type>, Delegate> _conversions;
        private readonly IDictionary<string, object> _properties;

        /// <summary>
        /// 
        /// </summary>
        public AppBuilder()
        {
            _properties = new Dictionary<string, object>();
            _conversions = new Dictionary<Tuple<Type, Type>, Delegate>();
            _middleware = new List<Tuple<Type, Delegate, object[]>>();

            _properties[Constants.BuilderAddConversion] = new Action<Delegate>(AddSignatureConversion);
            _properties[Constants.BuilderDefaultApp] = NotFound;

            SignatureConversions.AddConversions(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conversions"></param>
        /// <param name="properties"></param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        internal AppBuilder(
            IDictionary<Tuple<Type, Type>, Delegate> conversions,
            IDictionary<string, object> properties)
        {
            _properties = properties;
            _conversions = conversions;
            _middleware = new List<Tuple<Type, Delegate, object[]>>();
        }

        /// <summary>
        /// Contains arbitrary properties which may added, examined, and modified by
        /// components during the startup sequence. 
        /// </summary>
        public IDictionary<string, object> Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Adds a middleware node to the OWIN function pipeline. The middleware are
        /// invoked in the order they are added: the first middleware passed to Use will
        /// be the outermost function, and the last middleware passed to Use will be the
        /// innermost.
        /// </summary>
        /// <param name="middleware">
        /// The middleware parameter determines which behavior is being chained into the
        /// pipeline. 
        /// 
        /// If the middleware given to Use is a Delegate, then it will be invoked with the "next app" in 
        /// the chain as the first parameter. If the delegate takes more than the single argument, 
        /// then the additional values must be provided to Use in the args array.
        /// 
        /// If the middleware given to Use is a Type, then the public constructor will be 
        /// invoked with the "next app" in the chain as the first parameter. The resulting object
        /// must have a public Invoke method. If the object has constructors which take more than
        /// the single "next app" argument, then additional values may be provided in the args array.
        /// </param>
        /// <param name="args">
        /// Any additional args passed to Use will be passed as additional values, following the "next app"
        /// parameter, when the OWIN call pipeline is build.
        /// 
        /// They are passed as additional parameters if the middleware parameter is a Delegate, or as additional
        /// constructor arguments if the middle parameter is a Type.
        /// </param>
        /// <returns>
        /// The IAppBuilder itself is returned. This enables you to chain your use statements together.
        /// </returns>
        public IAppBuilder Use(object middleware, params object[] args)
        {
            _middleware.Add(ToMiddlewareFactory(middleware, args));
            return this;
        }

        /// <summary>
        /// The New method creates a new instance of an IAppBuilder. This is needed to create
        /// a tree structure in your processing, rather than a linear pipeline. The new instance share the
        /// same Properties, but will be created with a new, empty middleware list.
        /// 
        /// To create a tangent pipeline you would first call New, followed by several calls to Use on 
        /// the new builder, ending with a call to Build on the new builder. The return value from Build
        /// will be the entry-point to your tangent pipeline. This entry-point may now be added to the
        /// main pipeline as an argument to a switching middleware, which will either call the tangent
        /// pipeline or the "next app", based on something in the request.
        /// 
        /// That said - all of that work is typically hidden by a middleware like Map, which will do that
        /// for you.
        /// </summary>
        /// <returns>The new instance of the IAppBuilder implementation</returns>
        public IAppBuilder New()
        {
            return new AppBuilder(_conversions, _properties);
        }

        /// <summary>
        /// The Build is called at the point when all of the middleware should be chained
        /// together. This is typically done by the hosting component which created the app builder,
        /// and does not need to be called by the startup method if the IAppBuilder is passed in.
        /// </summary>
        /// <param name="returnType">
        /// The Type argument indicates which calling convention should be returned, and
        /// is typically typeof(<typeref name="Func&lt;IDictionary&lt;string,object&gt;, Task&gt;"/>) for the OWIN
        /// calling convention.
        /// </param>
        /// <returns>
        /// Returns an instance of the pipeline's entry point. This object may be safely cast to the
        /// type which was provided
        /// </returns>
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
                throw new ArgumentException(Resources.Exception_ConversionTakesOneParameter, "conversion");
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
            if (!_properties.TryGetValue(Constants.BuilderDefaultApp, out app))
            {
                app = NotFound;
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
                string.Format(CultureInfo.CurrentCulture, Resources.Exception_NoConversionExists, app.GetType(), signature), 
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
            MethodInfo signatureMethod = signature.GetMethod(Constants.Invoke);
            ParameterInfo[] signatureParameters = signatureMethod.GetParameters();

            MethodInfo[] methods = app.GetType().GetMethods();
            foreach (MethodInfo method in methods)
            {
                if (method.Name != Constants.Invoke)
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
            if (middlewareObject == null)
            {
                throw new ArgumentNullException("middlewareObject");
            }

            Delegate middlewareDelegate = middlewareObject as Delegate;
            if (middlewareDelegate != null)
            {
                return Tuple.Create(GetParameterType(middlewareDelegate), middlewareDelegate, args);
            }

            Tuple<Type, Delegate, object[]> factory = ToInstanceMiddlewareFactory(middlewareObject, args);
            if (factory != null)
            {
                return factory;
            }

            factory = ToGeneratorMiddlewareFactory(middlewareObject, args);
            if (factory != null)
            {
                return factory;
            }

            if (middlewareObject is Type)
            {
                return ToConstructorMiddlewareFactory(middlewareObject, args, ref middlewareDelegate);
            }

            throw new NotSupportedException(Resources.Exception_MiddlewareNotSupported + (middlewareObject ?? string.Empty).ToString());
        }

        // Instance pattern: public void Initialize(AppFunc next, string arg1, string arg2), public Task Invoke(IDictionary<...> env)
        private static Tuple<Type, Delegate, object[]> ToInstanceMiddlewareFactory(object middlewareObject, object[] args)
        {
            MethodInfo[] methods = middlewareObject.GetType().GetMethods();
            foreach (MethodInfo method in methods)
            {
                if (method.Name != Constants.Initialize)
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

                // DynamicInvoke can't handle a middleware with multiple args, just push the args in via closure.
                Func<object, object> func = app =>
                {
                    object[] invokeParameters = new[] { app }.Concat(args).ToArray();
                    method.Invoke(middlewareObject, invokeParameters);
                    return middlewareObject;
                };

                return Tuple.Create<Type, Delegate, object[]>(parameters[0].ParameterType, func, new object[0]);
            }
            return null;
        }

        // Delegate nesting pattern: public AppFunc Invoke(AppFunc app, string arg1, string arg2)
        private static Tuple<Type, Delegate, object[]> ToGeneratorMiddlewareFactory(object middlewareObject, object[] args)
        {
            MethodInfo[] methods = middlewareObject.GetType().GetMethods();
            foreach (MethodInfo method in methods)
            {
                if (method.Name != Constants.Invoke)
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
                Delegate middlewareDelegate = Delegate.CreateDelegate(funcType, middlewareObject, method);
                return Tuple.Create(parameters[0].ParameterType, middlewareDelegate, args);
            }
            return null;
        }

        // Type Constructor pattern: public Delta(AppFunc app, string arg1, string arg2)
        private static Tuple<Type, Delegate, object[]> ToConstructorMiddlewareFactory(object middlewareObject, object[] args, ref Delegate middlewareDelegate)
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

            throw new MissingMethodException(middlewareType.FullName,
                string.Format(CultureInfo.CurrentCulture, Resources.Exception_NoConstructorFound, args.Length + 1));
        }

        private static bool TestArgForParameter(Type parameterType, object arg)
        {
            return (arg == null && !parameterType.IsValueType) ||
                parameterType.IsInstanceOfType(arg);
        }
    }
}
