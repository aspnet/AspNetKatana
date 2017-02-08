// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Owin;

namespace Microsoft.Owin.Hosting.ServerFactory
{
    /// <summary>
    /// The basic ServerFactory contract.
    /// </summary>
    public class ServerFactoryAdapter : IServerFactoryAdapter
    {
        private readonly IServerFactoryActivator _activator;
        private readonly Type _serverFactoryType;
        private object _serverFactory;

        /// <summary>
        /// Creates a wrapper around the given server factory instance.
        /// </summary>
        /// <param name="serverFactory"></param>
        public ServerFactoryAdapter(object serverFactory)
        {
            if (serverFactory == null)
            {
                throw new ArgumentNullException("serverFactory");
            }

            _serverFactory = serverFactory;
            _serverFactoryType = serverFactory.GetType();
            _activator = null;
        }

        /// <summary>
        /// Creates a wrapper around the given server factory type.
        /// </summary>
        /// <param name="serverFactoryType"></param>
        /// <param name="activator"></param>
        public ServerFactoryAdapter(Type serverFactoryType, IServerFactoryActivator activator)
        {
            if (serverFactoryType == null)
            {
                throw new ArgumentNullException("serverFactoryType");
            }
            if (activator == null)
            {
                throw new ArgumentNullException("activator");
            }

            _serverFactoryType = serverFactoryType;
            _activator = activator;
        }

        /// <summary>
        /// Calls the optional Initialize method on the server factory.
        /// The method may be static or instance, and may accept either
        /// an IAppBuilder or the IAppBuilder.Properties IDictionary&lt;string, object&gt;.
        /// </summary>
        /// <param name="builder"></param>
        public virtual void Initialize(IAppBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            MethodInfo initializeMethod = _serverFactoryType.GetMethod("Initialize", new[] { typeof(IAppBuilder) });
            if (initializeMethod != null)
            {
                if (!initializeMethod.IsStatic && _serverFactory == null)
                {
                    _serverFactory = _activator.Activate(_serverFactoryType);
                }
                initializeMethod.Invoke(_serverFactory, new object[] { builder });
                return;
            }

            initializeMethod = _serverFactoryType.GetMethod("Initialize", new[] { typeof(IDictionary<string, object>) });
            if (initializeMethod != null)
            {
                if (!initializeMethod.IsStatic && _serverFactory == null)
                {
                    _serverFactory = _activator.Activate(_serverFactoryType);
                }
                initializeMethod.Invoke(_serverFactory, new object[] { builder.Properties });
                return;
            }
        }

        /// <summary>
        /// Calls the Create method on the server factory.
        /// The method may be static or instance, and may accept the AppFunc and the 
        /// IAppBuilder.Properties IDictionary&lt;string, object&gt;.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public virtual IDisposable Create(IAppBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            // TODO: AmbiguousMatchException is throw if there are multiple Create methods. Loop through them and try each.
            MethodInfo serverFactoryMethod = _serverFactoryType.GetMethod("Create");
            if (serverFactoryMethod == null)
            {
                // TODO: More detailed error message.
                throw new MissingMethodException("ServerFactory", "Create");
            }

            // TODO: IAppBuilder support? Initialize supports it.

            ParameterInfo[] parameters = serverFactoryMethod.GetParameters();
            if (parameters.Length != 2)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_ServerFactoryParameterCount, _serverFactoryType));
            }
            if (parameters[1].ParameterType != typeof(IDictionary<string, object>))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_ServerFactoryParameterType, _serverFactoryType));
            }

            // let's see if we don't have the correct callable type for this server factory
            object app = builder.Build(parameters[0].ParameterType);

            if (!serverFactoryMethod.IsStatic && _serverFactory == null)
            {
                _serverFactory = _activator.Activate(_serverFactoryType);
            }
            return (IDisposable)serverFactoryMethod.Invoke(_serverFactory, new[] { app, builder.Properties });
        }
    }
}
